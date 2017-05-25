local core = require "silly.core"
local db = require "db"
local proto = require "protocol.client"
local router = require "router"
local login = require "logind"
local gaterpc = require "gaterpc"

router.start(proto)

require "account"

core.start(function()
	local ok = db.start()
	print("db start", ok)
	ok = login:start()
	print("logind start", ok)
	ok = gaterpc:start()
	print("gaterpc start", ok)
end)

