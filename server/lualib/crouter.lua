local clientproto = require "protocol.client"

local R = {}
local router = {}

function R.reg(name, cb)
	local cmd = clientproto:querytag(name)
	assert(cmd, name)
	router[cmd] = cb;
end

function R.get(cmd)
	return router[cmd]
end

return R

