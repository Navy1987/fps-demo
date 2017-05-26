local core = require "silly.core"
local env = require "silly.env"
local rpc = require "saux.rpc"
local proto = require "protocol.server"
local rand = math.random

local TIMER = 1000
local TIMEOUT = 5000

local uid_token = {}
local expire_uid = {}

local arpc_session = { session = false }

local M = {}

for i = 0, TIMEOUT // TIMER do
	expire_uid[i] = {}
end

local function wheel(time)
	local wheel = time % TIMEOUT
	return wheel // TIMER
end

local function expire()
	local wid = wheel(core.current())
	local e = expire_uid[wid]
	for k, v in pairs(e) do
		print("expired token", v)
		uid_token[v] = nil
		e[k] = nil
	end
	core.timeout(TIMER, expire)
end

function M.alloctoken(uid)
	local tk = rand(1, 10000)
	uid_token[uid] = tk
	local wid = wheel(core.current() + TIMEOUT)
	local e = expire_uid[wid]
	e[#e + 1] = uid
	return tk
end

function M.check(uid, session)
	local s = uid_token[uid]
	if s and s == session then
		uid_token[uid] = s + 1
		return true
	end
	return false
end

function M.kick(uid)
	local s = uid_token[uid]
	uid_token[uid] = nil
end

function M.start()
	print("gate token start")
	core.timeout(TIMER, expire)
end

return M

