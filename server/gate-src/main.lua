local core = require "silly.core"
local env = require "silly.env"
local token = require "token"
local bridge = require "bridge"
local agent = require "agent"

core.start(function()
	bridge.start(agent)
	token.start()
end)

