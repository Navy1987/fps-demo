local core = require "silly.core"
local env = require "silly.env"
local msg = require "saux.msg"
local const = require "const"
local wire = require "virtualsocket.wire"
local serverproto = require "protocol.server"
local clientproto = require "protocol.client"

local M = {}

local EVENT = 0

local ERR = {
	cmd = 0,
	err = 0
}

local register_req = {
	kink = false,
	handler = {}
}
local subscribe_req = {
	event = 0
}

local KEEPALIVE = 1000

local gate_inst = {}
local gate_close = {}

local uid_online = {}
local uid_hook = {}

local client_router = {}
local server_router = {}

local close_cb = function() end

local client_decode = wire.inter_decode
local client_encode = wire.server_encode

local function sendclient(uid, cmd, data)
	local inst = uid_online[uid]
	if not inst then
		print("sendclient uid offline", uid)
		return false
	end
	local str = client_encode(clientproto, uid, cmd, data)
	return inst:send(str)
end

local function sendserver(uid, cmd, data)
	local inst = uid_online[uid]
	if not inst then
		print("sendserver uid offline", uid)
		return false
	end
	local str = client_encode(serverproto, uid, cmd, data)
	return inst:send(str)
end

local function multicastserver(cmd, data)
	local success = true
	local str = client_encode(serverproto, 0, cmd, data)
	for _, v in pairs(gate_inst) do
		local ok = v:send(str)
		success = success and ok
	end
	return success
end

local function socket_register(send)
	print("socket_register")
	local ok = multicastserver("s_register", register_req)
	assert(ok, "channel.start")
	return ok
end

local function subscribe_event()
	subscribe_req.event = EVENT
	local ok = multicastserver("s_subscribe", subscribe_req)
	assert(ok, "channel.subscribe")
	return ok
end

local function keepalive_timer()
	local reg = client_encode(serverproto, 0, "s_register", register_req)
	local scr = client_encode(serverproto, 0, "s_subscribe", subscribe_req)
	local len = #gate_close
	local cnt = 0
	for k = cnt, 1, -1 do
		local v = gate_close[k]
		local ok, status = core.pcall(msg.connect, v)
		print("keepalive_timer gateid", v.gateid, ok, status)
		if ok and status then
			v:send(reg)
			v:send(scr)
			--TODO:synchroize online status
			table.remove(gate_close, k)
			cnt = cnt + 1
		end
	end

	if #gate_close > 0 then
		core.timeout(KEEPALIVE, keepalive_timer)
	end
end

local function auth_data_cb(gateid)
	return function(fd, d, sz)
		local uid, cmd, data = client_decode(d, sz)
		local handler = client_router[cmd]
		local proto
		if handler then
			proto = clientproto
			local req = proto:decode(cmd, data:sub(8+1))
			local ok, err = core.pcall(handler, uid, req, gateid)
			if not ok then
				print("data", uid, err)
			end
		else
			handler = server_router[cmd]
			assert(handler, cmd)
			proto = serverproto
			local req = proto:decode(cmd, data:sub(8+1))
			local ok, err = core.pcall(handler, uid, req, gateid)
			if not ok then
				print("data", uid, err)
			end
		end
	end
end

local function data_cb(gateid)
	return function(fd, d, sz)
		local uid, cmd, data = client_decode(d, sz)
		local handler = client_router[cmd]
		local proto
		if handler then
			proto = clientproto
			local req = proto:decode(cmd, data:sub(8+1))
			local ok, err = core.pcall(handler, uid, req)
			if not ok then
				print("data", uid, err)
			end
		else
			handler = server_router[cmd]
			assert(handler, cmd)
			proto = serverproto
			local req = proto:decode(cmd, data:sub(8+1))
			local ok, err = core.pcall(handler, uid, req, gateid)
			if not ok then
				print("data", uid, err)
			end
		end
	end
end

local function create_data_cb(data)
	local gateid = 0
	while true do
		gateid = gateid + 1
		local port = env.get("gate_inter_" .. gateid)
		if not port then
			break
		end
		local inst = msg.createclient {
			addr = port,
			accept = function(fd, addr)
				print("accept", addr)
			end,
			close = function(fd, errno)
				print("close", fd, errno)
				gate_close[#gate_close + 1] = gate_inst[gateid]
				core.timeout(KEEPALIVE, keepalive_timer)
			end,
			data = data(gateid)
		}
		print("create gate", gateid)
		inst.gateid = gateid
		gate_inst[gateid] = inst
	end
end

local function start()
	for _, v in pairs(gate_inst) do
		local ok = v:connect()
		if not ok then
			print("start gate id", v.gateid, "fail")
			gate_close[#gate_close + 1] = v
		end
	end
	if #gate_close > 0 then
		core.timeout(KEEPALIVE, keepalive_timer)
	end
	local handler = register_req.handler
	local i = 1
	for k, v in pairs(client_router) do
		handler[i] = k
		i = i + 1
		print("channel register:", k)
	end
	ok = socket_register()
	return ok
end

function M.startauth()
	create_data_cb(auth_data_cb)
	register_req.kind = 1
	return start()
end

function M.startlogic()
	create_data_cb(data_cb)
	register_req.kind = 2
	return start()
end

local event_key = "OC"
--[[
	'O' -- open
	'C' -- close
]]--

function M.subscribe(event)
	EVENT = 0
	for i = 1, #event do
		local n = event:byte(i)
		if n == event_key:byte(1) then -- open
			EVENT = EVENT | const.EVENT_OPEN
		elseif n == event_key:byte(2) then -- close
			EVENT = EVENT | const.EVENT_CLOSE
		end
	end
	return subscribe_event()
end

function M.reg_client(name, cb)
	local cmd = clientproto:querytag(name)
	assert(cmd, name)
	assert(cb, name)
	client_router[cmd] = cb
end

function M.reg_server(name, cb)
	local cmd = serverproto:querytag(name)
	assert(cmd, name)
	server_router[cmd] = cb
end

function M.sendserver(uid, typ, dat)
	return sendserver(uid, typ, dat)
end

function M.kick(uid)
	return sendserver(uid, "s_kick", const.EMPTY)
end

function M.send(uid, typ, dat)
	return sendclient(uid, typ, dat)
end

function M.error(uid, typ, err)
	local cmd = clientproto:querytag(typ)
	assert(cmd)
	ERR.cmd = cmd
	ERR.err = err
	sendclient(uid, "error", ERR)
end


function M.hookclose(cb)
	close_cb = cb
end

function M.hookuclose(uid, cb)
	uid_hook[uid] = cb
end

function M.errorgate(gateid, uid, typ, err)
	local cmd = clientproto:querytag(typ)
	assert(cmd)
	ERR.cmd = cmd
	ERR.err = err
	local str = client_encode(clientproto, uid, "error", ERR)
	return gate_inst[gateid]:send(str)
end

function M.sendgate(gateid, uid, cmd, data)
	local str = client_encode(clientproto, uid, cmd, data)
	return gate_inst[gateid]:send(str)
end

function M.sendservergate(gateid, uid, typ, dat)
	local str = client_encode(serverproto, uid, typ, dat)
	return gate_inst[gateid]:send(str)
end

function M.kickgate(gateid, uid)
	return M.sendservergate(gateid, uid, "s_kick", const.EMPTY)
end


local function s_connect(uid, _, gateid)
	print("connect", uid)
	uid_online[uid] = gate_inst[gateid]
end

local function s_close(uid, _, gateid)
	print("close", uid)
	local cb = uid_hook[uid]
	close_cb(uid, gateid)
	uid_online[uid] = nil
	uid_hook[uid] = nil
	if cb then
		cb(uid)
	end
end

M.reg_server("s_connect", s_connect)
M.reg_server("s_close", s_close)

return M

