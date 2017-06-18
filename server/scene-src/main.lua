local core = require "silly.core"
local channel = require "channel"
local scene = require "scene"

core.start(function()
	local ok = channel.subscribe("")
	print("channel subscribe", ok)
	local ok = channel.start()
	print("channel start", ok)
	scene:start()
end)

