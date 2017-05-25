local core = require "silly.core"
local env = require "silly.env"
local rpc = require "saux.rpc"
local const = require "const"
local serverproto = require "protocol.server"

local gateid = assert(env.get("gateid"), "gateid")

local M = {}

--[[
local msg = require "saux.msg"
local wire = require "virtualsocket.wire"
local clientproto = require "protocol.client"
local string = string


local IDX = 0

local gate_inst
local broker_inst

local TIMEOUT = 15000

local gate_decode = wire.gate_decode
local inter_decode = wire.inter_decode
local inter_encode = wire.inter_encode
local server_encode = wire.server_encode

local OPEN = assert(serverproto:querytag("s_connect"))
local CLOSE = assert(serverproto:querytag("s_close"))

--auth
local auth_stamp = {
	stamp = false
}

-- uid bind
local gate_fd_pend = {}
local gate_fd_uid = {}
local gate_uid_fd = {}

--handler
local auth_gate_fd = {}
local auth_handler = {}
local logic_handler = {}

--scrible
local subscribe_open_set = {}
local subscribe_close_set = {}

local function send_broker(broker_fd, uid, cmd, req)
	local dat = server_encode(serverproto, uid, cmd, req)
	broker_inst:send(broker_fd, dat)
end

local function broker_event(set, uid, cmd)
	for k, _ in pairs(set) do
		broker_inst:send(k, string.pack("<I4I4", uid, cmd))
	end
end

local function broker_open(uid)
	return broker_event(subscribe_open_set, uid, OPEN)
end

local function broker_close(uid)
	return broker_event(subscribe_close_set, uid, CLOSE)
end

local function broker_online(broker_fd)
	local nty = {
		uid = {}
	}
	local tbl = nty.uid
	for uid, _ in pairs(gate_uid_fd) do
		tbl[#tbl + 1] = uid
	end
	send_broker(broker_fd, 0, "s_online", nty)
end


local function gate_data(gate_fd, cmd, data)
	local uid = gate_fd_uid[gate_fd]
	local broker_fd
	if not uid then
		broker_fd = auth_handler[cmd]
		assert(auth_gate_fd[broker_fd])
		uid = gate_fd
		assert(broker_fd, cmd)
		local stamp = gate_fd_pend[gate_fd]
		if not stamp then
			IDX = IDX + 1
			auth_stamp.stamp = IDX
			gate_fd_pend[gate_fd] = IDX
			send_broker(broker_fd, gate_fd, "s_authstamp", auth_stamp)
		end
	else
		broker_fd = logic_handler[cmd]
		assert(not auth_gate_fd[broker_fd])
		assert(broker_fd, cmd)
	end
	--TODO:notify server busy when broker_fd is nil
	broker_inst:send(broker_fd, inter_encode(uid, data))
end

local function gate_clear(gate_fd)
	local uid = gate_fd_uid[gate_fd]
	gate_fd_uid[gate_fd] = nil
	gate_fd_pend[gate_fd] = nil
	if uid then
		broker_close(uid)
		gate_uid_fd[uid] = nil
	end
end


gate_inst = msg.createserver {
	addr = env.get("gate_port_" .. gateid),
	accept = function(fd, addr)
		print("accept", fd, addr)
	end,
	close = function(fd, errno)
		gate_clear(fd)
		print("close", fd, errno)
	end,
	data = function(fd, d, sz)
		local cmd, data = gate_decode(d, sz)
		gate_data(fd, cmd, data)
	end
}


local T = {}

local function clear_handler(broker_fd)
	local tbl
	subscribe_open_set[broker_fd] = nil
	subscribe_close_set[broker_fd] = nil
	if (auth_gate_fd[broker_fd]) then
		auth_gate_fd[broker_fd] = nil
		tbl = auth_handler
	else
		tbl = logic_handler
	end
	for k, v in pairs(tbl) do
		if v == broker_fd then
			tbl[k] = nil
		end
	end
end

broker_inst = msg.createserver {
	addr = env.get("gate_broker_" .. gateid),
	accept = function(fd, addr)
		print("accept", fd, addr)
	end,
	close = function(fd, errno)
		clear_handler(fd)
		print("close", fd, errno)
	end,
	data = function(fd, d, sz)
		local uid, cmd, data = inter_decode(d, sz)
		local handler = T[cmd]
		if handler then
			local req = serverproto:decode(cmd, data:sub(8+1))
			handler(fd, uid, req)
			return
		end
		local client_fd
		if (auth_gate_fd[fd]) then
			client_fd = uid
		else
			client_fd = gate_uid_fd[uid]
		end
		if not client_fd then
			print("client_fd", uid, "disconnect")
			return
		end
		gate_inst:send(client_fd, data:sub(4 + 1))
	end
}


T[serverproto:querytag("s_register")] = function(broker_fd, uid, req)
	local fill
	print("register", broker_fd, req.kind)
	if req.kind == 1 then --auth handler
		fill = auth_handler
		auth_gate_fd[broker_fd] = true
	else
		fill = logic_handler
	end
	for _, v in pairs(req.handler) do
		fill[v] = broker_fd
		print("register cmd:", v)
	end
end

T[serverproto:querytag("s_authok")] = function(broker_fd, gate_fd, req)
	local uid = req.uid
	local acktime = req.stamp
	local time = gate_fd_pend[gate_fd]
	if not time then
		print("s_authok", gate_fd, "has close")
		return
	end
	if time ~= acktime then
		print("s_authok stamp not same", time, acktime)
		return
	end
	print("auth ok", gate_fd, uid)
	gate_fd_pend[gate_fd] = nil
	gate_fd_uid[gate_fd] = uid
	gate_uid_fd[uid] = gate_fd
	broker_open(uid)
end

T[serverproto:querytag("s_kick")] = function(broker_fd, uid, req)
	local gate_fd = gate_uid_fd[uid]
	if not gate_fd then
		return
	end
	print("s_kick", uid)
	gate_inst:close(gate_fd)
	gate_clear(gate_fd)
end

T[serverproto:querytag("s_subscribe")] = function(broker_fd, uid, req)
	local event = req.event
	local open_mask = const.EVENT_OPEN
	local close_mask = const.EVENT_CLOSE
	local notify = false
	if (event & open_mask) == open_mask then
		subscribe_open_set[broker_fd] = true
		notify = true
		print("broker_fd subscribe open", broker_fd)
	end
	if (event & close_mask) == close_mask then
		subscribe_close_set[broker_fd] = true
		notify = true
		print("broker_fd subscribe open", broker_fd)
	end
	if notify then
		broker_online(broker_fd)
	end
end
function M.start()
	local ok = gate_inst:start()
	print("gate start:", ok)
	local ok = broker_inst:start()
	print("broker start:", ok)
end
]]--

----------------------------------------------------

function M.start()

end

return M


