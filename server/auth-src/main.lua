local core = require "silly.core"
local channel = require "virtualsocket.channel"
local db = require "db"

require "account"

core.start(function()
	local ok = channel.startauth()
	print("channel start", ok)
	ok = channel.subscribe("C")
	print("channel subscribe", ok)
	ok = db.start()
	print("db start", ok)
end)

