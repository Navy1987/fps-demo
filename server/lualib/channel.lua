local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local const = require "const"
local wire = require "virtualsocket.wire"
local cproto = require "protocol.client"
local sproto = require "protocol.server"
local spack = string.pack

local M = {}

local TIMER = 1000
local TIMEOUT = 5000

local gate_channel = {}

local client_handler = {}
local server_handler = {}

local online_uid_gate = {}

local event_connect

local inter_decode = wire.inter_decode
local inter_encode = wire.server_encode

-----socket protocol
local sr_register = {
	rpc = false,
	id = false,
	typ = false,
	handler = {}
}

local sr_online = {
	rpc = false
}

local event_key = "OC"
--[[
	'O' -- open
	'C' -- close
]]--

local function cproto_encode(cmd, ack)
	cmd = cproto:querytag(cmd)
	local hdr = string.pack("<I4", cmd)
	local body = cproto:encode(cmd, ack)
	return hdr .. body
end

local function wheel(time)
	local wheel = time % TIMEOUT
	return wheel // TIMER
end

local function createchannel(gateid)
	local addr = env.get("gate_broker_" .. gateid)
	if not addr then
		return nil
	end
	local ID = gateid
	local rpc_suspend = {}
	local rpc_wheel = {}

	for i = 0, TIMEOUT//TIMER do
		rpc_wheel[i] = {}
	end

	local function wakeupall()
		for k, co in pairs(rpc_suspend) do
			core.wakeup(co)
			rpc_suspend[k] = nil
		end
	end

	local function waitfor(id)
		local co = core.running()
		local w = wheel(core.current() + TIMEOUT)
		rpc_suspend[id] = co
		local expire = rpc_wheel[w]
		expire[#expire + 1] = id;
		return core.wait()
	end
	local client

	local function checkconnect()
		local t = 1000
		while true do
			local ok = client:connect()
			print("connect gateid", gateid, ok)
			if ok then
				client:call("sr_register", sr_register)
				if event_connect then
					event_connect(client)
				end
				return
			end
			core.sleep(t)
		end
	end

	client = msg.createclient {
		addr = addr,
		close = function(fd, errno)
			print("close", fd, errno)
			wakeupall()
			core.fork(checkconnect)
		end,
		data = function(fd, d, sz)
			local uid, cmd, str = inter_decode(d, sz)
			local func = client_handler[cmd]
			if func then
				local req = cproto:decode(cmd, str:sub(8+1))
				func(uid, req, gateid)
			else
				func = assert(server_handler[cmd], cmd)
				local req = sproto:decode(cmd, str:sub(8+1))
				func(uid, req, gateid)
			end
		end
	}
	client.gateid = gateid

	client.wakeup = function(ack)
		local s = assert(ack.rpc)
		local co = rpc_suspend[s]
		if not co then
			print("channel wakeup session timeout", s, gateid)
			return
		end
		core.wakeup(co, ack)
		rpc_suspend[s] = nil
	end

	client.timer = function ()
		local now = core.current()
		local w = wheel(now)
		local expire = rpc_wheel[w]
		for k, v in pairs(expire) do
			local co = rpc_suspend[v]
			if co then
				print("channel timeout session", v)
				rpc_suspend[v] = nil
				core.wakeup(co)
			end
			expire[k] = nil
		end
	end

	client.call = function(self, cmd, req)
		if type(cmd) == "string" then
			cmd = sproto:querytag(cmd)
		end
		ID = ID + 1
		req.rpc = ID
		local dat = sproto:encode(cmd, req);
		local hdr = spack("<I4I4", 0, cmd)
		local ok = self:send(hdr .. dat)
		if not ok then
			return
		end
		return waitfor(ID)
	end
	print("[channel] creat", addr, client, client.gateid)
	return client
end

local function timer()
	--rpc timer
	for _, v in pairs(gate_channel) do
		v.timer()
	end
	core.timeout(TIMER, timer)
end
core.timeout(TIMER, timer)

------------rpc handler

local function rpc_ack(uid, ack, gateid)
	local g = gate_channel[gateid]
	g.wakeup(ack)
end

local rpc_ack_cmd = {
	"sa_register",
	"sa_session",
	"sa_kick",
	"sa_online",
	"sa_gateonline",
	"sa_sceneonline",
	"sa_locateplayer",
}

for _, v in pairs(rpc_ack_cmd) do
	server_handler[sproto:querytag(v)] = rpc_ack
end

-------------interface

function M.reg_client(cmd, cb)
	cmd = cproto:querytag(cmd)
	client_handler[cmd] = cb
end

function M.reg_server(cmd, cb)
	cmd = sproto:querytag(cmd)
	server_handler[cmd] = cb
end

function M.gate(gateid)
	return gate_channel[gateid]
end

function M.gates()
	return gate_channel
end

function M.hook_connect(e)
	event_connect = e
end

local register = function(gate)
	return gate:call("sr_register", sr_register)
end

function M.online(gate)
	local ack = gate:call("sr_online", sr_online)
	if not ack then
		return nil
	end
	return ack.uid
end

function M.attach(uid, gateid)
	online_uid_gate[uid] = gate_channel[gateid]
end

function M.detach(uid)
	online_uid_gate[uid] = nil
end

local gate_set = {}
function M.multicast(uids, cmd, packet)
	local ack = {
		uids = false,
		data = false
	}
	local dat = cproto_encode(cmd, packet)
	for k, _ in pairs(uids) do
		local gate = online_uid_gate[k]
		if gate then
			local g = gate_set[gate]
			if not g then
				g = {}
				gate_set[gate] = g
			end
			g[#g + 1] = k
		else
			print("incorrect gate of", k)
		end
		uids[k] = nil
	end
	ack.data = dat
	for gate, uids in pairs(gate_set) do
		ack.uids = uids
		local dat = inter_encode(sproto, 0, "s_multicast", ack)
		gate:send(dat)
		gate_set[gate] = nil
	end
	return
end

function M.send(uid, cmd, ack)
	local ch = online_uid_gate[uid]
	if not ch then
		return false
	end
	local dat = inter_encode(cproto, uid, cmd, ack)
	return ch:send(dat)
end

function M.sendgate(gateid, uid, cmd, ack)
	local ch = gate_channel[gateid]
	if not ch then
		return false
	end
	local dat = inter_encode(cproto, uid, cmd, ack)
	return ch:send(dat)
end

function M.start(id, typ)
	local count = 0
	local gateid = 1
	sr_register.id = id
	sr_register.typ = typ
	local l = sr_register.handler
	for k, _ in pairs(client_handler) do
		l[#l + 1] = k
	end
	while true do
		local inst = createchannel(gateid)
		if not inst then
			break
		end
		gate_channel[gateid] = inst
		count = count + 1
		if inst:connect() and register(inst) then
			print("[channel] connect gate", inst.addr, gateid)
			if event_connect then
				event_connect(inst)
			end
		end
		gateid = gateid + 1
	end
	return count
end

return M

