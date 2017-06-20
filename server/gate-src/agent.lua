local core = require "silly.core"
local env = require "silly.env"
local np = require "netpacket"
local const = require "const"
local token = require "token"
local bridge = require "bridge"
local broker = require "broker"
local cproto = require "protocol.client"
local sproto = require "protocol.server"
local spack = string.pack
local sunpack = string.unpack
local cmd = {}

local COORD = tonumber(env.get("coord_resolution"))

local M = {}
local agent_mt = {__index = M}

function M.create(self, gatefd)
	--default the value
	local obj = {
		gatefd = gatefd,
		uid = false,
		roleserver = false,
		sceneserver = false,
		position = false,
		rotation = nil,
		--
	}
	setmetatable(obj, agent_mt)
	print("[agent] create gatefd", gatefd, obj)
	return obj
end

function M.gc(self)
	self.gatefd = false
	self.uid = false
	self.roleserver = false
	self.sceneserver = false
	self.position = false
end

local logout_packet = sproto:encode("s_logout", const.EMPTY)
local logout_cmd = sproto:querytag("s_logout")
logout_packet = spack("<I4", logout_cmd) .. logout_packet

function M.logout(self)
	local sceneid = self.sceneserver
	if sceneid and self.uid then
		local fd = broker.getscene(self.sceneserver)
		if fd then
			bridge.forwardbroker(fd, self.uid, logout_packet)
		end
	end
	self.uid = false
end

function M.gatedata(self, fd, d, sz)
	local str = core.tostring(d, sz)
	np.drop(d)
	local c = sunpack("<I4", str)
	local func = cmd[c]
	if not func and self.uid then
		local brokerfd = broker.getbroker(self, c)
		if not brokerfd then
			print("[agent] broker close of cmd:", cmd)
			return
		end
		bridge.forwardbroker(brokerfd, self.uid, str)
	elseif not self.uid then
		local dat = str:sub(4 + 1)
		local req = cproto:decode(c, dat)
		assert(self.gatefd == fd)
		print("[agent] gatefd", self.gatefd, self)
		func(self, req)
		assert(func, c)
	end
end

--str == 'uid|cmd|packet'
function M.brokerdata(self, fd, str)
	bridge.forwardclient(self.gatefd, str:sub(4+1))
end

-------------------cmd handler
local a_login_gate = {
	pos = {
		x = 1000,
		z = 1000,
	}
}
cmd[cproto:querytag("r_login_gate")] = function(self, req)
	local uid = req.uid
	local ok = token.check(uid, req.session)
	if not ok then
		print("uid:", req.uid, " login incorrect session:", req.session)
		bridge.kickgate(fd)
		return
	end
	self.uid = uid
	self.position = {
		x = 1 * COORD,
		z = 1 * COORD,
	}
	--TODO:hash it
	self.roleserver = 1
	self.sceneserver = 1
	bridge.login(self, uid)
	bridge.sendgate(self.gatefd, "a_login_gate", a_login_gate)
end

return M

