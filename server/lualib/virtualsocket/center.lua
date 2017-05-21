local core = require "silly.core"
local env = require "silly.env"
local rpc = require "saux.rpc"
local const = require "const"
local msg = require "saux.msg"
local wire = require "virtualsocket.wire"
local serverproto = require "protocol.server"
local clientproto = require "protocol.client"

local M = {}
local rpc_inst
local gate_inst
local transfer_inst

local TIMEOUT = 15000
local gate_decode = wire.gate_decode
local inter_decode = wire.inter_decode
local inter_encode = wire.inter_encode


--auth
local gate_fd_uid = {}
local gate_fd_pend = {}


local gate_uid_fd = {}
local auth_handler = {}
local logic_handler = {}

local function clear_handler(fd)
	for k, v in pairs(auth_handler) do
		auth_handler[k] = nil
	end
	for k, v in pairs(logic_handler) do
		logic_handler[k] = nil
	end
end

local function forward_client(uid, _, data)
	--todo:handler auth
	local fd = gate_uid_fd[uid]
	if not fd then
		fd = uid
	end
	gate_inst:send(fd, data:sub(4 + 1))
end

local login_cmd = assert(clientproto:querytag("r_login"))

local function do_login(gate_fd, data)
	local time_before = assert(gate_fd_pend[gate_fd])
	local ack = rpc_inst:call("s_auth", data)
	local time_after = gate_fd_pend[gate_fd]
	if not (time_after and (time_after == time_before)) then
		print("gate fd", gate_fd, "has be reassign")
		return
	end
	local uid = data.uid
	if not uid then
		--TODO:notify client login fail
		return
	end
	gate_inst:send(fd, clientproto:encode("a_login", const.EMPTY))
end

local function forward_transfer(gate_fd, cmd, data)
	local uid = gate_uid_fd[gate_fd]
	local handler
	if not uid then
		if cmd == login_cmd then
			return do_login(gate_fd, data)
		end
		uid = gate_fd
		handler = auth_handler
	else
		handler = logic_handler
	end
	local transfer_fd = handler[cmd]
	--TODO:notify server busy when transfer_fd is nil
	assert(transfer_fd, cmd)
	transfer_inst:send(transfer_fd, inter_encode(uid, data))
end

rpc_inst = rpc.createclient {
	addr = env.get "auth_inter",
	proto = serverproto,
	timeout = 5000,
	close = function(fd, errno)
		print("center rpc close", fd, errno)
	end
}

gate_inst = msg.createserver {
	addr = env.get("gate_port"),
	accept = function(fd, addr)
		print("accept", addr)
		assert(not gate_fd_pend[fd], "gate fd reassign")
		gate_fd_pend[fd] = core.current()
	end,
	close = function(fd, errno)
		local uid = gate_fd_uid[fd]
		gate_fd_uid[fd] = nil
		gate_fd_pend[fd] = nil
		if uid then
			gate_uid_fd[uid] = nil
		end
		print("close", fd, errno)
	end,
	data = function(fd, d, sz)
		local cmd, data = gate_decode(d, sz)
		forward_transfer(fd, cmd, data)
	end
}


local T = {}

T[serverproto:querytag("s_register")] = function(fd, uid, cmd, req)
	local fill
	print("register")
	if req.kind == 1 then --auth handler
		fill = auth_handler
	else
		fill = logic_handler
	end
	for _, v in pairs(req.handler) do
		auth_handler[v] = fd
		print("register cmd:", v)
	end
end

T[serverproto:querytag("s_auth")] = function(_, fd, cmd, req)
	local uid = req.uid
	local time = gate_fd_pend[fd]
	if not time then
		return
	end
	--TODO:process the fd reassign, use the temp uid
	gate_fd_pend[fd] = nil
	gate_fd_uid[fd] = req.uid
	gate_uid_fd[req.uid] = req.fd
end

T[serverproto:querytag("s_kick")] = function(_, ud, cmd, req)
	if req.kind == 1 then --auth kick
		gate_fd_pend[ud] = nil
		gate_fd_uid[ud] = nil
		assert(ud == req.uid)
	else
		local uid = req.uid
		assert(ud == uid)
		assert(not gate_fd_pend[uid])
		local fd = gate_uid_fd[uid]
		gate_fd_pend[fd] = nil
		gate_fd_uid[fd] = nil
	end
	gate_inst:close(fd)
end

transfer_inst = msg.createserver {
	addr = env.get("gate_inter"),
	accept = function(fd, addr)
		print("accept", addr)
	end,
	close = function(fd, errno)
		clear_handler(fd)
		print("close", fd, errno)
	end,
	data = function(fd, d, sz)
		local uid, cmd, data = inter_decode(d, sz)
		print(fd, uid, cmd)
		local handler = T[cmd]
		if handler then
			local req = serverproto:decode(cmd, data:sub(8+1))
			handler(fd, uid, cmd, req)
			return
		end
		forward_client(uid, cmd, data)
	end
}

function M.start()
	local ok = gate_inst:start()
	print("gate start:", ok)
	local ok = transfer_inst:start()
	print("transfer start:", ok)
	local ok = rpc_inst:connect()
	print("rpc start:", ok)
end

return M


