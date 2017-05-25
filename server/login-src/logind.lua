local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local router = require "router"
local np = require "netpacket"
local const = require "const"
local proto = require "protocol.client"

local M = {}

local ERR = {
	cmd = 0,
	err = 0
}

local close_callback

local S = msg.createserver {
	addr = env.get("login_port"),
	accept = function(fd, addr)
		print("accept", fd, addr)
	end,
	close = function(fd, errno)
		close_callback(fd)
		print("close", fd, errno)
	end,
	data = function(fd, d, sz)
		local str = core.tostring(d, sz)
		np.drop(d)
		local len = #str
		assert(len >= 4)
		local data
		local cmd = string.unpack("<I4", str)
		if len > 4 then
			data = proto:decode(cmd, str:sub(4+1))
		else
			data =  const.EMPTY
		end
		local cb = assert(router.get(cmd), cmd)
		cb(fd, data)
	end
}

function M.event_close(close)
	close_callback = close
end

local function send(fd, cmd, dat)
	if type(cmd) == "string" then
		cmd = proto:querytag(cmd)
	end
	local hdr = string.pack("<I4", cmd)
	local body = proto:encode(cmd, dat)
	return S:send(fd, hdr .. body)

end

M.send = send

function M.error(fd, cmd, err)
	local cmd = proto:querytag(cmd)
	assert(cmd)
	ERR.cmd = cmd
	ERR.err = err
	return send(fd, "error", ERR)
end

function M.start(close)
	return S:start()
end

return M

