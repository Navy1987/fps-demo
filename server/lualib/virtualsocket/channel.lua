local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local const = require "const"
local wire = require "virtualsocket.wire"
local serverproto = require "protocol.server"
local clientproto = require "protocol.client"

local M = {}

local EVENT = 0

local ERR = {
	cmd = 0,
	err = 0
}

local register_req = {
	kink = false,
	handler = {}
}


local KEEPALIVE = 1000

local uid_online = {}

local client_router = {}
local server_router = {}

local client_decode = wire.inter_decode
local client_encode = wire.server_encode

local inst

local function sendclient(uid, cmd, data)
	local str = client_encode(clientproto, uid, cmd, data)
	return inst:send(str)
end

local function sendserver(uid, cmd, data)
	local str = client_encode(serverproto, uid, cmd, data)
	return inst:send(str)
end

local function socket_register()
	print("socket_register")
	local ok = sendserver(0, "s_register", register_req)
	assert(ok, "channel.start")
	return ok
end

local function subscribe_event()
	local req = {
		event = EVENT
	}
	local ok = sendserver(0, "s_subscribe", req)
	assert(ok, "channel.subscribe")
	return ok
end

local function keepalive_timer()
	local ok, status = core.pcall(inst.connect, inst)
	print("keepalive_timer", ok, status)
	if ok and status then
		socket_register()
		subscribe_event()
		return
	end
	core.timeout(KEEPALIVE, keepalive_timer)
end

inst = msg.createclient {
	addr = env.get("gate_inter"),
	accept = function(fd, addr)
		print("accept", addr)
	end,
	close = function(fd, errno)
		print("close", fd, errno)
		core.timeout(KEEPALIVE, keepalive_timer)
	end,
	data = function(fd, d, sz)
		local uid, cmd, data = client_decode(d, sz)
		local handler = client_router[cmd]
		local proto
		if handler then
			proto = clientproto
		else
			handler = server_router[cmd]
			proto = serverproto
		end
		assert(handler, cmd)
		local req = proto:decode(cmd, data:sub(8+1))
		local ok, err = core.pcall(handler, uid, req)
		if not ok then
			print("data", uid, err)
		end
	end
}

print("channel create:", inst)

local function start(typ)
	local ok = inst:connect()
	if not ok then
		core.timeout(KEEPALIVE, keepalive_timer)
		return ok
	end
	register_req.kind = typ
	local handler = register_req.handler
	local i = 1
	for k, v in pairs(client_router) do
		handler[i] = k
		i = i + 1
		print("channel register:", k)
	end
	ok = socket_register()
	return ok
end

function M.startauth()
	return start(1)
end

function M.startlogic()
	return start(2)
end

local event_key = "OC"
--[[
	'O' -- open
	'C' -- close
]]--

function M.subscribe(event)
	EVENT = 0
	for i = 1, #event do
		local n = event:byte(i)
		if n == event_key:byte(1) then -- open
			EVENT = EVENT | const.EVENT_OPEN
		elseif n == event_key:byte(2) then -- close
			EVENT = EVENT | const.EVENT_CLOSE
		end
	end
	return subscribe_event()
end

function M.reg_client(name, cb)
	local cmd = clientproto:querytag(name)
	assert(cmd, name)
	assert(cb, name)
	client_router[cmd] = cb
end

function M.reg_server(name, cb)
	local cmd = serverproto:querytag(name)
	assert(cmd, name)
	server_router[cmd] = cb
end

function M.send(uid, typ, dat)
	return sendclient(uid, typ, dat)
end

function M.sendserver(uid, typ, dat)
	return sendserver(uid, typ, dat)
end

function M.error(uid, typ, err)
	local cmd = clientproto:querytag(typ)
	assert(cmd)
	ERR.cmd = cmd
	ERR.err = err
	sendclient(uid, "error", ERR)
end

function M.hookclose(uid, cb)
	--TODO:hook close
end


local function s_connect(uid, _)
	print("connect", uid)
	uid_online[uid] = true
end

local function s_close(uid, _)
	print("close", uid)
	uid_online[uid] = false
end

M.reg_server("s_connect", s_connect)
M.reg_server("s_close", s_close)

return M

