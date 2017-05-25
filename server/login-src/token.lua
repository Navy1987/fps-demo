local core = require "silly.core"
local crypt = require "crypt"

local TIMER = 1000
local TIMEOUT = 5000

local token_uid = {}
local expire_token = {}

local M = {}

for i = 1, TIMEOUT // TIMER do
	expire_token[i] = {}
end

local function wheel(time)
	local wheel = time % TIMEOUT
	return wheel // TIMER
end

function M.alloctoken(uid, timeout)
	local tk = tostring(uid)
	tk = crypt.hmac(tk, uid)
	token_uid[tk] = uid
	local wid = wheel(core.current() + TIMEOUT)
	local e = expire_token[wid]
	e[#e + 1] = tk
	return tk
end

function M.tokenuid(token)
	return token_uid[token]
end

local function expire()
	local wid = wheel(core.current())
	local e = expire_token[wid]
	for k, v in pairs(e) do
		print("expired token", v)
		token_uid[v] = nil
		e[k] = nil
	end
	core.timeout(TIMEOUT, expire)
end

core.timeout(TIMEOUT, expire)

return M

