local log = require "log"
local channel = require "virtualsocket.channel"
local errno = require "protocol.errno"

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


local function join(uid, req)
	print("join", uid, req.join)
	req.uid = uid
	if req.join == 0 then
		arena[uid] = nil
	else
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

channel.hookclose(close)
channel.reg_client("r_join", join)
channel.reg_client("r_sync", sync)

