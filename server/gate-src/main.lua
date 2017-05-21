local core = require "silly.core"
local env = require "silly.env"
local center = require "virtualsocket.center"

core.start(function()
	center.start()
end)

