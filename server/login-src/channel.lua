local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
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

local inter_decode = wire.inter_decode


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
		for k, _ in pairs(rpc_suspend) do
			core.wakeup(k)
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

	local client = msg.createclient {
		addr = addr,
		close = function(fd, errno)
			print("close", fd, errno)
			wakeupall()
		end,
		data = function(fd, d, sz)
			local uid, cmd, str = inter_decode(d, sz)
			local func = client_handler[cmd]
			if func then
				local req = cproto:decode(cmd, str:sub(8+1))
				func(uid, cmd, req)
			else
				func = assert(server_handler[cmd], cmd)
				local req = sproto:decode(cmd, str:sub(8+1))
				func(gateid, cmd, req)
			end
		end
	}

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
	print("creat channel", addr)
	return client
end

local function timer()
	for _, v in pairs(gate_channel) do
		v.timer()
	end
	core.timeout(TIMER, timer)
end
core.timeout(TIMER, timer)

------------rpc handler
local function rpc_ack(gateid, cmd, ack)
	local g = gate_channel[gateid]
	g.wakeup(ack)
end

local rpc_ack_cmd = {
	"sa_session",
	"sa_kick",
}

for _, v in pairs(rpc_ack_cmd) do
	server_handler[sproto:querytag(v)] = rpc_ack
end

-------------

function M.gate(gateid)
	return gate_channel[gateid]
end

function M.start()
	local count = 0
	local gateid = 1
	while true do
		local inst = createchannel(gateid)
		if not inst then
			break
		end
		if inst:connect() then
			gate_channel[gateid] = inst
			count = count + 1
			print("gate channel connect ok", inst.addr)
		end
		gateid = gateid + 1
	end
	return count
end

return M

