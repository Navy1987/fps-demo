local core = require "silly.core"
local np = require "netpacket"
local lib = require "virtualsocket.lib"

local gc = lib.gc
local client = {}
local clientmt = {__index = client, __gc == gc}
local NIL = {}


local client_decode = lib.server_decode
local client_encode = lib.server_encode

---server
local function clientcb(sc)
	local EVENT = {}
	local queue = np.create()
	function EVENT.accept(fd, portid, addr)
		assert(not "never come here")
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
		local uid, cmd, data = client_decode(sc.proto, d, sz)
		local ok, err = core.pcall(sc.data, uid, cmd, data)
		if not ok then
			print("[vc] dispatch socket", err)
		end
	end

	return function (type, fd, message, ...)
		queue = np.message(queue, message)
		assert(EVENT[type])(fd, ...)
	end
end

local function wakeupall(self)
	local q = self.connectqueue
	for k, v in pairs(q) do
		core.wakeup(v)
		q[k] = nil
	end
end

local function checkconnect(self)
       if self.fd and self.fd >= 0 then
		return self.fd
	end
	if not self.fd then	--disconnected
		self.fd = -1
		local ok
		local fd = core.connect(self.addr, clientcb(self))
		if not fd then
			self.fd = false
			ok = false
		else
			self.fd = fd
			ok = true
			self.reconnect()
		end
		wakeupall(self)
		return ok
	else
		local co = core.running()
		local t = self.connectqueue
		t[#t + 1] = co
		core.wait()
		return self.fd and self.fd > 0
	end
end

function client.sendmsg(self, uid, cmd, data)
	checkconnect(self)
	local str = client_encode(self.proto, uid, cmd, data)
	return core.write(self.fd, np.pack(str))
end

function client.sendproto(self, proto, cmd, data)
	checkconnect(self)
	local str = client_encode(proto, 0, cmd, data)
	return core.write(self.fd, np.pack(str))
end

function client.create(config)
	local obj = {
		fd = false,
		connectqueue = {}
	}
	for k, v in pairs(config) do
		obj[k] = v
	end
	setmetatable(obj, clientmt)
	return obj
end


function client.start(self)
	local fd = core.connect(self.addr, clientcb(self))
	self.fd = fd
	return fd
end

function client.close(self)
	gc(self)
end

return client


