local core = require "silly.core"
local channel = require "virtualsocket.channel"

require "battle"

core.start(function()
	local ok = channel.startlogic()
	print("channel start", ok)
end)

