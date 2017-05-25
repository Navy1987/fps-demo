local env = require "silly.env"
local rpc = require "saux.rpc"
local proto = require "protocol.server"

local gate_rpc = {}
local M = {}

local function createrpc(gateid)
	local addr = env.get("gate_rpc_" .. gateid)
	if not addr then
		return nil
	end
	print("createrpc", addr)
	return rpc.createclient {
		addr = addr,
		proto = proto,
		timeout = 5000,
		close = function(fd, errno)
			print("close", fd, errno)
		end
	}
end

function M.gate(gateid)
	return gate_rpc[gateid]
end

function M.start()
	local count = 0
	local gateid = 1
	while true do
		local inst = createrpc(gateid)
		if not inst then
			break
		end
		if inst:connect() then
			gate_rpc[gateid] = inst
			count = count + 1
			print("gaterpc create gateid", gateid)
		end
		gateid = gateid + 1
	end
	return count
end

return M

