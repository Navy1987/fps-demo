local proto
local R = {}
local router = {}

function R.start(p)
	proto = assert(p)
end

function R.register(name, cb)
	local cmd = proto:querytag(name)
	assert(cmd, name)
	router[cmd] = cb;
end

function R.get(cmd)
	return router[cmd]
end

return R

