local core = require "silly.core"
local np = require "netpacket"
local M = {}

local NIL = {}

function M.gc(obj)
	if not obj.fd then
		return
	end
	if obj.fd < 0 then
		return
	end
	core.close(obj.fd)
	obj.fd = false
end

---------for gate.lua
--return cmd, data
function M.gate_decode(d, sz)
	local str = core.tostring(d, sz)
	np.drop(d)
	local len = #str
	assert(len >= 4)
	return string.unpack("<I4", str), str
end

-----for transfer.lua
--return data
function M.inter_encode(uid, dat)
	local hdr = string.pack("<I4", uid)
	return hdr .. dat
end

--return uid, cmd
function M.inter_decode(d, sz)
	local str = core.tostring(d, sz)
	np.drop(d)
	local len = #str
	assert(len >= 8)
	local uid, cmd = string.unpack("<I4I4", str)
	return uid, cmd, str
end

----for server.lua
--return uid, cmd, data
function M.server_decode(proto, d, sz)
	local str = core.tostring(d, sz)
	np.drop(d)
	local len = #str
	assert(len >= 8)
	local data
	local uid, cmd = string.unpack("<I4I4", str)
	if (len > 8) then
		data = proto:decode(cmd, str:sub(8+1))
	else
		data = NIL
	end
	return uid, cmd, data
end

--return data
function M.server_encode(proto, uid, cmd, dat)
	if type(cmd) == "string" then
		cmd = proto:querytag(cmd)
	end
	local hdr = string.pack("<I4I4", uid, cmd)
	local body = proto:encode(cmd, dat)
	return hdr .. body
end

return M

