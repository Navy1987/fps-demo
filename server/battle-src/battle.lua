local log = require "log"
local channel = require "channel"
local errno = require "protocol.errno"

local M = {}


local arena = {}

local function broadcast(typ, req)
	for k, v in pairs(arena) do
		channel.send(k, typ, req)
	end
end

local function close(uid)
	if not arena[uid] then
		return
	end
	arena[uid] = nil
	local req = {
		uid =  uid,
		join = 0
	}
	broadcast("a_join", req)
end


local function join(uid, req, gateid)
	print("join", uid, req.join)
	req.uid = uid
	if req.join == 0 then
		arena[uid] = nil
		channel.detach(uid)
	else
		channel.attach(uid, gateid)
		arena[uid] = true
		for k, v in pairs(arena) do
			req.uid = k
			channel.send(uid, "a_join", req)
		end
		req.uid = uid
	end
	broadcast("a_join", req)
end

local function sync(uid, req)
	print("sync", uid, req.pos.x/100000, req.pos.y/100000, req.pos.z/100000)
	req.uid = uid
	broadcast("a_sync", req)
end

local function s_login(uid, _, gate)
	print("s_login", uid)
end

local function s_logout(uid, _)
	print("s_logout", uid)
	channel.detach(uid)
end

channel.reg_server("s_login", s_login)
channel.reg_server("s_logout", s_logout)
channel.reg_client("r_join", join)
channel.reg_client("r_sync", sync)

local function reconnect(gate)
	local uids = channel.online(gate)
	if not uids then
		return
	end
	print("online", uids)
	for k, v in pairs(uids) do
		print(k, v)
	end
end

function M.start()
	for _, v in pairs(channel.gates()) do
		print("gates")
		reconnect(v)
	end
end

return M
