local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local const = require "const"
local token = require "token"
local np = require "netpacket"
local cproto = require "protocol.client"
local sproto = require "protocol.server"
local wire = require "virtualsocket.wire"
local spack = string.pack

local M = {}
local CCMD = {}
local SCMD = {}

local gate_inst
local broker_inst

local broker_handler = {}

local subscribe_login = {}
local subscribe_logout = {}

local online_gatefd_uid = {}
local online_uid_gatefd = {}

local cmd_decode = wire.gate_decode
local broker_decode = wire.inter_decode
local broker_encode = wire.inter_encode
local server_encode = wire.server_encode

local gateid = assert(env.get("gateid"), "gateid")

local function sendbroker(broker_fd, gatefd, data)
	local uid = online_gatefd_uid[gatefd]
	if not uid then
		gate_inst:close(gatefd)
		return
	end
	local pk = broker_encode(uid, data)
	broker_inst:send(broker_fd, pk)
end

local function sendclient(gatefd, cmd, ack)
	if type(cmd) == "string" then
		cmd = cproto:querytag(cmd)
	end
	local hdr = spack("<I4", cmd)
	local body = cproto:encode(cmd, ack)
	gate_inst:send(gatefd, hdr .. body)
end

------------

local LOGIN = sproto:querytag("s_login")
local LOGOUT = sproto:querytag("s_logout")

local function gate_clear(gatefd)
	local uid = online_gatefd_uid[gatefd]
	online_gatefd_uid[gatefd] = nil
	if uid then
		local str = spack("<I4I4", uid, LOGOUT)
		online_uid_gatefd[uid] = nil
		for v, _ in pairs(subscribe_logout) do
			print("subscribe logout", v)
			broker_inst:send(v, str)
		end
	end
end

local function gate_login(gatefd, uid)
	online_gatefd_uid[gatefd] = uid
	online_uid_gatefd[uid] = gatefd
	local str = spack("<I4I4", uid, LOGIN)
	for v, _ in pairs(subscribe_login) do
		broker_inst:send(v, str)
	end
end

local function broker_clear(brokerfd)
	print("broker_clear")
	subscribe_login[brokerfd] = nil
	subscribe_logout[brokerfd] = nil
	for k, v in pairs(broker_handler) do
		if v == brokerfd then
			broker_handler[k] = nil
		end
	end
end

local function uid_kick(uid)
	local gatefd = online_uid_gatefd[uid]
	print("uid_kick", uid, gatefd)
	if gatefd then
		gate_inst:close(gatefd)
		gate_clear(gatefd)
	end
end

gate_inst = msg.createserver {
	addr = env.get("gate_port_" .. gateid),
	accept = function(fd, addr)
		print("[gate] accept", fd, addr)
	end,
	close = function(fd, errno)
		gate_clear(fd)
		print("[gate] close", fd, errno)
	end,
	data = function(fd, d, sz)
		local cmd, data = cmd_decode(d, sz)
		local broker_fd = broker_handler[cmd]
		if broker_fd then
			sendbroker(broker_fd, fd, data)
		else
			local req = cproto:decode(cmd, data:sub(4+1))
			assert(CCMD[cmd], cmd)(fd, req)
		end
	end
}
local a_login_gate = {
	pos = {
		x = 1000,
		z = 1000,
	}
}
CCMD[cproto:querytag("r_login_gate")] = function(fd, req)
	local uid = req.uid
	local ok = token.check(uid, req.session)
	if not ok then
		print("uid:", req.uid, " login incorrect session:", req.session)
		gate_inst:close(fd)
		return
	end
	gate_login(fd, uid)
	sendclient(fd, "a_login_gate", a_login_gate)
end

----------------------------------------------------
broker_inst = msg.createserver {
	addr = env.get("gate_broker_" .. gateid),
	accept = function(fd, addr)
		print("broker accept", fd, addr)
	end,
	close = function(fd, errno)
		print("broker close", fd, errno)
		broker_clear(fd)
	end,
	data = function(fd, d, sz)
		local uid, cmd, data = broker_decode(d, sz)
		if uid == 0 then
			local req = sproto:decode(cmd, data:sub(8+1))
			assert(SCMD[cmd], cmd)(fd, req)
			return
		end
		local gatefd = online_uid_gatefd[uid]
		if gatefd then
			print("[gate] forward uid:", uid)
			gate_inst:send(gatefd, data:sub(4+1))
		else
			print("disconnect uid", uid)
		end
	end
}

local function send_server(fd, cmd, ack)
	cmd = sproto:querytag(cmd)
	local pk = server_encode(sproto, 0, cmd, ack)
	broker_inst:send(fd, pk)
end

SCMD[sproto:querytag("sr_register")] = function(fd, req)
	if (req.event & const.EVENT_OPEN) == const.EVENT_OPEN then
		subscribe_login[fd] = true
	end
	if (req.event & const.EVENT_CLOSE) == const.EVENT_CLOSE then
		subscribe_logout[fd] = true
	end
	for _, v in pairs(req.handler) do
		broker_handler[v] = fd
		print(string.format("cmd:0x%x fd:%d", v, fd))
	end
	req.event = nil
	req.handler = nil
	send_server(fd, "sa_register", req)
	print("sr_register")
end

SCMD[sproto:querytag("sr_session")] = function(fd, req)
	local uid = req.uid
	local tk = token.alloctoken(uid)
	req.ud = nil
	req.session = tk
	send_server(fd, "sa_session", req)
	print("loginserver fetch session")
end

SCMD[sproto:querytag("sr_kick")] = function(fd, req)
	local uid = req.uid
	print("kick", uid)
	token.kick(uid)
	uid_kick(uid)
	send_server(fd, "sa_kick", req)
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
		local fd = online_uid_gatefd[uid]
		print("Multicast", uid)
		if not fd then
			core.freemulti(m, sz)
		else
			local ok = broker_inst:multicast(fd, m, sz)
		end
	end
end

----------------------------------------------------

function M.start()
	local ok = gate_inst:start();
	print("gate listen:", gate_inst.addr, ok)
	ok = broker_inst:start()
	print("broker listen:", broker_inst.addr, ok)
end

return M


