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
	end
	broadcast("a_join", req)
end

local function sync(uid, req)
	req.uid = uid
	broadcast("a_sync", req)
end

local function r_battleinfo(uid, req)
	local ack = {
		uid = {}
	}
	local t = ack.uid
	for k, _ in pairs(arena) do
		t[#t + 1] = k
	end
	channel.send(uid, "a_battleinfo", ack)
end

local function r_shoot(uid, req)
	print("shoot", uid)
	broadcast("a_shoot", req)
end

channel.reg_client("r_join", join)
channel.reg_client("r_sync", sync)
channel.reg_client("r_battleinfo", r_battleinfo)
channel.reg_client("r_shoot", r_shoot)

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

local function s_login(uid, _, gate)
	print("s_login", uid)
end

local function s_logout(uid, _)
	print("s_logout", uid)
	close(uid)
	channel.detach(uid)
end

channel.reg_server("s_login", s_login)
channel.reg_server("s_logout", s_logout)

return M
