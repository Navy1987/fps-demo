local server = require "server"
local log = require "log"
local router = require "router"
local player = require "player"
local errno = require "protocol.errno"

local arena = {}

local function close(fd)
	arena[fd] = nil
end

local function broadcast(typ, req)
	for k, v in pairs(arena) do
		local fd = player.fd(k)
		server.send(fd, typ, req)
	end
end

local function join(fd, req)
	print("join")
	local uid = player.uid(fd)
	if req.join == 0 then
		server.hookclose(fd, nil)
		req.uid = uid
		broadcast("a_join", req)
		arena[uid] = nil
	else
		server.hookclose(fd, close)
		arena[uid] = true
		for k, v in pairs(arena) do
			req.uid = k
			server.send(fd, "a_join", req);
		end
	end
end

local function sync(fd, req)
	print("sync")
	local uid = player.uid(fd)
	req.uid = uid
	broadcast("a_sync", req)
end


router.reg("r_join", join)
router.reg("r_sync", sync)

