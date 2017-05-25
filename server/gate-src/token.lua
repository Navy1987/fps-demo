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

local gateid = assert(env.get("gateid"), "gateid")
local gate_port = "gate_rpc_" .. gateid
gate_port = assert(env.get(gate_port), gate_port)


local function wheel(time)
	local wheel = time % TIMEOUT
	return wheel // TIMER
end

local function alloctoken(uid)
	local tk = rand(1, 10000)
	uid_token[uid] = tk
	local wid = wheel(core.current() + TIMEOUT)
	local e = expire_uid[wid]
	e[#e + 1] = uid
	return tk
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

local CMD = {}

print("gate rpc port", gate_port)

local server = rpc.createserver {
	addr = gate_port,
	proto = proto,
	accept = function(fd, addr)
		print("accept", fd, addr)
	end,
	close = function(fd, errno)
		print("close", fd, errno)
	end,
	call = function(fd, cmd, msg)
		return assert(CMD[cmd])(fd, cmd, msg)
	end,
}

CMD[proto:querytag("rrpc_session")] = function (fd, cmd, msg)
	local uid = msg.uid
	local session = alloctoken(uid)
	arpc_session.session = session
	print("gate uid session", uid, session)
	return "arpc_session", arpc_session
end

function M.check(uid, session)
	local s = uid_token[uid]
	if s and s == session then
		uid_token[uid] = s + 1
		return true
	end
	return false
end

function M.start()
	local ok = server:listen()
	print("gate token start:", ok)
	core.timeout(TIMER, expire)
end

return M

