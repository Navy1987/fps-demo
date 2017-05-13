local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local clientproto = require "protocol.client"
local server
server = msg.createserver {
	addr = env.get("gate_port"),
	proto = clientproto,
	accept = function(fd, addr)
		print("accept", addr)
	end,
	close = function(fd, errno)
		print("close", fd, errno)
	end,
	data = function(fd, cmd, data)
		print("data", fd, cmd, data.hello, data.world)
		data.world = 8;
		server:send(fd, "a_foo", data)
	end
}

core.start(function()
	ok = server:start()
	print("server start", ok)
end)

