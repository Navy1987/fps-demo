local core = require "silly.core"
local env = require "silly.env"
local token = require "token"
local center = require "center"

core.start(function()
	center.start()
	token.start()
end)

