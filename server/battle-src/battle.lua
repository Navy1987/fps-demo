local channel = require "channel"
local log = require "log"
local errno = require "protocol.errno"

local arena = {}

local function broadcast(typ, req)
	for k, v in pairs(arena) do
		channel.send(k, typ, req)
	end
end

local function close(uid)
	arena[uid] = nil
	local req = {
		uid =  uid,
		join = 0
	}
	channel.hookclose(uid, nil)
	broadcast("a_join", req)
end


local function join(uid, req)
	print("join", uid, req.join)
	req.uid = uid
	if req.join == 0 then
		channel.hookclose(uid, nil)
		arena[uid] = nil
	else
		channel.hookclose(uid, close)
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


channel.register("r_join", join)
channel.register("r_sync", sync)

