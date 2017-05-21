local core = require "silly.core"
local env = require "silly.env"
local server = require "virtualsocket.server"
local serverproto = require "protocol.server"
local clientproto = require "protocol.client"

local M = {}

local ERR = {
	cmd = 0,
	err = 0
}

local client_router = {}
local server_router = {}

local register_req = {
	kink = false,
	handler = {}
}

local inst

local function socket_register()
	local ok = inst:sendproto(serverproto, "s_register", register_req)
	assert(ok, "channel.start")
	return ok
end
local KEEPALIVE = 1000
local function keepalive_timer()
	local ok, status = core.pcall(server.checkconnect, inst)
	print("keepalive_timer", ok, status)
	if ok and status then
		return
	end
	core.timeout(KEEPALIVE, keepalive_timer)
end

inst = server.create {
	proto = clientproto,
	addr = env.get("gate_inter"),
	reconnect = socket_register,
	accept = function(fd, addr)
		print("accept", addr)
	end,
	close = function(fd, errno)
		print("close", fd, errno)
		core.timeout(KEEPALIVE, keepalive_timer)
	end,
	data = function(uid, cmd, data)
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
	local ok = inst:start()
	if not ok then
		return ok
	end
	local handler = register_req.handler
	local i = 1
	for k, v in pairs(client_router) do
		handler[i] = k
		i = i + 1
		print("channel register:", k)
	end
	ok = socket_register()
	core.timeout(KEEPALIVE, keepalive_timer)
	return ok
end

function M.startauth()
	return start(1)
end

function M.startlogic()
	return start(2)
end

function M.reg_client(name, cb)
	local cmd = clientproto:querytag(name)
	assert(cmd, name)
	client_router[cmd] = cb
end

function M.reg_server(name, cb)
	local cmd = serverproto:querytag(name)
	assert(cmd, name)
	server_router[cmd] = cb
end

function M.send(uid, typ, dat)
	return inst:sendmsg(uid, typ, dat)
end

function M.error(uid, typ, err)
	local cmd = clientproto:querytag(typ)
	assert(cmd)
	ERR.cmd = cmd
	ERR.err = err
	inst:sendmsg(uid, "error", ERR)
end

function M.hookclose(uid, cb)
	--TODO:hook close
end

return M

