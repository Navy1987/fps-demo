local core = require "silly.core"
local channel = require "channel"

require "battle"

core.start(function()
	local ok = channel.startauth()
	print("channel start", ok)
end)

