local server = require "server"
local log = require "log"
local router = require "router"
local player = require "player"
local errno = require "protocol.errno"

local arena = {}

local function broadcast(typ, req)
	for k, v in pairs(arena) do
		local fd = player.fd(k)
		server.send(fd, typ, req)
	end
end

local function close(fd)
	local uid = player.uid(fd)
	arena[uid] = nil
	local req = {
		uid =  uid,
		join = 0
	}
	server.hookclose(fd, nil)
	broadcast("a_join", req)
end


local function join(fd, req)
	local uid = player.uid(fd)
	print("join", uid, req.join)
	req.uid = uid
	if req.join == 0 then
		server.hookclose(fd, nil)
		arena[uid] = nil
	else
		server.hookclose(fd, close)
		arena[uid] = true
		for k, v in pairs(arena) do
			req.uid = k
			server.send(fd, "a_join", req)
		end
		req.uid = uid
	end
	broadcast("a_join", req)
end

local function sync(fd, req)
	local uid = player.uid(fd)
	print("sync", uid, req.pos.x/100000, req.pos.y/100000, req.pos.z/100000)
	req.uid = uid
	broadcast("a_sync", req)
end


router.reg("r_join", join)
router.reg("r_sync", sync)

