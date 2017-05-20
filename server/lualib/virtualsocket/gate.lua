local core = require "silly.core"
local np = require "netpacket"
local lib = require "virtualsocket.lib"

local gc = lib.gc
local gateserver = {}
local gatemt = {__index = gateserver, __gc == gc}
local NIL = {}

local gate_decode = lib.gate_decode

---server
local function servercb(sc)
	local EVENT = {}
	sc.queue = np.create()
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
		local f, d, sz = np.pop(sc.queue)
		if not f then
			return
		end
		core.fork(EVENT.data)
		local cmd, data = gate_decode(d, sz)
		local ok, err = core.pcall(sc.data, f, cmd, data)
		if not ok then
			print("[vc] dispatch socket", err)
		end
	end

	return function (type, fd, message, ...)
		sc.queue = np.message(sc.queue, message)
		assert(EVENT[type])(fd, ...)
	end
end

local function sendmsg(self, fd, data)
	return core.write(fd, np.pack(data))
end

gateserver.sendmsg = sendmsg

function gateserver.create(config)
	local obj = {
		fd = false,
	}
	for k, v in pairs(config) do
		obj[k] = v
	end
	setmetatable(obj, gatemt)
	return obj

end

function gateserver.start(self)
	local fd = core.listen(self.addr, servercb(self))
	self.fd = fd
	return fd
end

function gateserver.stop(self)
	gc(self)
end

function gateserver.close(self, fd)
	np.clear(self.queue, fd)
	core.close(fd)
end

return gateserver

