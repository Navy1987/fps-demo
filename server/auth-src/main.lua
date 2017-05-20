local core = require "silly.core"
local channel = require "channel"
local db = require "db"

require "account"

core.start(function()
	local ok = channel.startauth()
	print("channel start", ok)
	ok = db.start()
	print("db start", ok)
end)

