local core = require "silly.core"
local channel = require "channel"
local battle = require "battle"

core.start(function()
	local ok = channel.subscribe("OC")
	print("channel subscribe", ok)
	local ok = channel.start()
	print("channel start", ok)
	battle:start()
end)

