local core = require "silly.core"
local np = require "netpacket"
local lib = require "virtualsocket.lib"

local gc = lib.gc
local msgserver = {}
local servermt = {__index = msgserver, __gc == gc}
local NIL = {}

local server_decode = lib.inter_decode
local server_encode = lib.inter_encode

---server
local function servercb(sc)
	local EVENT = {}
	local queue = np.create()
	function EVENT.accept(fd, portid, addr)
		local ok, err = core.pcall(sc.accept, fd, addr)
		if not ok then
			print("[vc] EVENT.accept", err)
			core.close(fd)
		end
	end

	function EVENT.close(fd, errno)
		local ok, err = core.pcall(assert(sc).close, fd, errno)
		if not ok then
			print("[vc] EVENT.close", err)
		end
	end

	function EVENT.data()
		local f, d, sz = np.pop(queue)
		if not f then
			return
		end
		core.fork(EVENT.data)
		local uid, cmd, data = server_decode(d, sz)
		local ok, err = core.pcall(sc.data, f, uid, cmd, data)
		if not ok then
			print("[vc] dispatch socket", err)
		end
	end

	return function (type, fd, message, ...)
		queue = np.message(queue, message)
		assert(EVENT[type])(fd, ...)
	end
end

local function sendmsg(self, fd, uid, data)
	return core.write(fd, np.pack(server_encode(uid, data)))
end

msgserver.sendmsg = sendmsg

function msgserver.create(config)
	local obj = {
		fd = false,
	}
	for k, v in pairs(config) do
		obj[k] = v
	end
	setmetatable(obj, servermt)
	return obj
end


function msgserver.start(self)
	local fd = core.listen(self.addr, servercb(self))
	self.fd = fd
	return fd
end

function msgserver.close(self)
	gc(self)
end

return msgserver


