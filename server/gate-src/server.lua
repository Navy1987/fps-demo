local env = require "silly.env"
local msg = require "saux.msg"
local router = require "router"
local clientproto = require "protocol.client"
local server

local M = {}
local hookclose = {}

local ERR = {
	cmd = 0,
	err = 0
}

server = msg.createserver {
	addr = env.get("gate_port"),
	proto = clientproto,
	accept = function(fd, addr)
		print("accept", addr)
	end,
	close = function(fd, errno)
		local cb = hookclose[fd]
		if cb then
			cb(fd, err)
		end
		print("close", fd, errno)
	end,
	data = function(fd, cmd, data)
		assert(router.get(cmd))(fd, data)
	end
}

function M.start()
	return server:start()
end

function M.hookclose(fd, cb)
	hookclose[fd] = cb
end

function M.send(fd, typ, obj)
	server:send(fd, typ, obj)
end

function M.error(fd, typ, err)
	local cmd = clientproto:querytag(typ)
	assert(cmd)
	ERR.cmd = cmd
	ERR.err = err
	print("cmd, err", cmd, err)
	server:send(fd, "error", ERR)
end

return M

