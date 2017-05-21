local core = require "silly.core"
local env = require "silly.env"
local rpc = require "saux.rpc"
local router = require "srouter"
local proto = require "protocol.server"

local M = {}

local session = {}
local id = 1

local server = rpc.createserver {
	addr = env.get "auth_inter",
	proto = proto,
	accept = function(fd, addr)
		print("accept", fd, addr)
	end,
	close = function(fd, errno)
		print("close", fd, errno)
	end,
	call = function(fd, cmd, msg)
		local handler = router.get(cmd)
		return assert(handler)(fd, cmd, msg)
	end,
}

function M.auth_token(uid)
	id = id + 1
	assert(not session[uid])
	session[uid] = id
	return tostring(id)

end


router.register("s_auth", function(fd, cmd, msg)
	local s = session[msg.uid]
	local token = tonumber(msg.token)
	msg.token = nil
	if not s or s ~= msg.token then
		msg.uid = 0
	end
	print("auth-server auth", msg.uid)
	return cmd, msg
end)

return M

