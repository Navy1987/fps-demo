local player = {}

local fd_info = {}
local uid_info = {}

function player.init(fd, uid)
	--TODO:load player info
	fd_info[fd] = uid
	uid_info[uid] = fd
end

function player.uid(fd)
	return fd_info[fd]
end

function player.fd(uid)
	return uid_info[uid]
end

return player

