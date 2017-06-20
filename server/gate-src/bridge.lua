local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local const = require "const"
local token = require "token"
local broker = require "broker"
local np = require "netpacket"
local cproto = require "protocol.client"
local sproto = require "protocol.server"
local spack = string.pack
local sunpack = string.unpack
local tremove = table.remove
local tinsert = table.insert
local agent

local M = {}
local SCMD = {}

local gate_inst
local broker_inst

local gatefd_agent = {}
local uid_agent = {}

local agent_pool = {}
local weak_mt = {__mode = "kv"}
setmetatable(agent_pool, weak_mt)

local function createagent(gatefd)
	local a = tremove(agent_pool)
	if not a then
		a = agent:create(gatefd)
	else
		a.gatefd = gatefd
	end
	return a
end

local function freeagent(a)
	a:gc()
	tinsert(agent_pool, a)
end

local gateid = assert(env.get("gateid"), "gateid")

M.forwardbroker = function(broker_fd, uid, data)
	local hdr = spack("<I4", uid)
	broker_inst:send(broker_fd, hdr .. data)
end

M.forwardclient = function(gatefd, data)
	gate_inst:send(gatefd, data)
end

M.sendgate = function (gatefd, cmd, ack)
	if type(cmd) == "string" then
		cmd = cproto:querytag(cmd)
	end
	local hdr = spack("<I4", cmd)
	local body = cproto:encode(cmd, ack)
	gate_inst:send(gatefd, hdr .. body)
end

local function uid_kick(uid)
	local a = uid_agent[uid]
	if not a then
		return
	end
	local gatefd = a.gatefd
	uid_agent[uid] = nil
	gatefd_agent[uid] = nil
	a:logout()
	gate_inst:close(gatefd)
end

M.kickgate = function (gatefd)
	local a = gatefd_agent[gatefd]
	gatefd_agent[gatefd] = nil
	if a.uid then
		uid_agent[a.uid] = nil
	end
	a:logout()
	gate_inst:close(gatefd)
end

M.login = function(agent, uid)
	uid_agent[uid] = agent
end

gate_inst = msg.createserver {
	addr = env.get("gate_port_" .. gateid),
	accept = function(fd, addr)
		print("[gate] client accept", fd, addr)
		gatefd_agent[fd] = createagent(fd)
	end,
	close = function(fd, errno)
		print("[gate] client close", fd, errno)
		local a = gatefd_agent[fd]
		a:logout()
		freeagent(a)
	end,
	data = function(fd, d, sz)
		print("[gate] client data", fd)
		local a = assert(gatefd_agent[fd], fd)
		a:gatedata(fd, d, sz)
	end
}

broker_inst = msg.createserver {
	addr = env.get("gate_broker_" .. gateid),
	accept = function(fd, addr)
		print("[gate] broker accept", fd, addr)
	end,
	close = function(fd, errno)
		print("[gate] broker close", fd, errno)
	end,
	data = function(fd, d, sz)
		print("[gate] broker data", fd)
		local str = core.tostring(d, sz)
		np.drop(d)
		assert(#str >= 8)
		local uid, cmd = sunpack("<I4I4", str)
		local a = uid_agent[uid]
		if a then
			a:brokerdata(cmd, str)
			return
		end
		if uid == 0 then
			local req = sproto:decode(cmd, str:sub(8 + 1))
			assert(SCMD[cmd], cmd)(fd, req)
			return
		end
		print("uid", uid "already logout")
	end
}

------------------socket protocol

local function send_server(broker_fd, cmd, ack)
	cmd = sproto:querytag(cmd)
	local dat = sproto:encode(cmd, ack)
	local hdr = spack("<I4I4", 0, cmd)
	broker_inst:send(broker_fd, hdr .. dat)
end

SCMD[sproto:querytag("sr_register")] = function(fd, req)
	broker.register(fd, req.id, req.typ, req.handler)
	req.event = nil
	req.handler = nil
	send_server(fd, "sa_register", req)
	print("[gate-bridge] sr_register", req.typ)
end

SCMD[sproto:querytag("sr_session")] = function(fd, req)
	local uid = req.uid
	local tk = token.alloctoken(uid)
	req.ud = nil
	req.session = tk
	send_server(fd, "sa_session", req)
	print("[gate-bridge] loginserver fetch session", tk)
end

SCMD[sproto:querytag("sr_kick")] = function(fd, req)
	local uid = req.uid
	token.kick(uid)
	uid_kick(uid)
	send_server(fd, "sa_kick", req)
	print("[gate-bridge] kick", uid)
end

SCMD[sproto:querytag("sr_online")] = function(fd, req)
	local tbl = {}
	req.uid = tbl
	for uid, _ in pairs(online_uid_gatefd) do
		tbl[#tbl + 1] = uid
	end
	send_server(fd, "sa_online", req)
	print("sr_online")
end

SCMD[sproto:querytag("s_multicast")] = function(fd, req)
	local uids = req.uids
	local dat = req.data
	local p, sz = np.pack(dat)
	local m = core.packmulti(p, sz, #uids)
	np.drop(p)
	for _, uid in pairs(uids) do
		local a = uid_agent[uid]
		print("Multicast", uid)
		if not a then
			core.freemulti(m, sz)
		else
			broker_inst:multicast(a.gatefd, m, sz)
		end
	end
end

SCMD[sproto:querytag("sr_sceneonline")] = function(fd, req)
	local sceneserver = req.sceneid
	local l = {}
	req.sceneid = nil
	req.uids = l
	for uid, agent in pairs(uid_agent) do
		if agent.sceneserver == sceneserver then
			l[#l + 1] = uid
		end
	end
	send_server(fd, "sa_sceneonline", req)
end

SCMD[sproto:querytag("sr_gateonline")] = function(fd, req)
	local u = {}
	req.uids = u
	for uid, _ in pairs(uid_agent) do
		u[#u + 1] = uid
	end
	send_server(fd, "sa_gateonline", req)
end

SCMD[sproto:querytag("sr_locateplayer")] = function(fd, req)
	local r_uids = req.uids
	local p_list = {}
	req.uids = nil
	req.players = p_list
	for _, uid in pairs(r_uids) do
		local a = uid_agent[uid]
		if a then
			p_list[#p_list + 1] = {
				uid = uid,
				pos = a.position,
				rot = a.rotation,
			}
		end
	end
	send_server(fd, "sa_locateplayer", req)
end

----------------------------------------------------

function M.start(agent_module)
	local ok = gate_inst:start();
	print("[gate] listen client:", gate_inst.addr, ok)
	ok = broker_inst:start()
	print("[gate] listen broker:", broker_inst.addr, ok)
	agent = agent_module
end

return M


