local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local server = require "server"
local log = require "log"
local db = require "db"

require "account"

core.start(function()
	log.file("gate.log")
	local lprint = log.print
	log.print = function(...)
		print(...)
		lprint(...)
	end
	local ok = db:start()
	print("db start", ok)
	ok = server:start()
	print("server start", ok)
end)

