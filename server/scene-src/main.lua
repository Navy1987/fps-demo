local core = require "silly.core"
local channel = require "channel"
local scene = require "scene"

core.start(function()
	local ok = channel.start(1, "scene")
	print("[scene] channel start", ok)
	local ok = scene:start()
	print("[scene] scene start", ok)
end)

