local core = require "silly.core"
local env = require "silly.env"
local const = require "const"
local log = require "log"
local channel = require "channel"
local errno = require "protocol.errno"

local SCENEID = assert(tonumber(env.get("sceneid")), "sceneid")
local GRID_SIZE = 10000;
local X_POWER = 10000
local X_OFFSET = 1 * X_POWER + 0
local Y_OFFSET = 1 * X_POWER + 1

local M = {}

local GRID = {
	--[grid_index] = {
	--	[uid] = true
	--}
}

local PLAYER = {
	--["uid"] = {
		--["uid"] = uid
		--["pos"] = pos
		--["grid"] = grid_index
	--}
}

local function grid_index(pos)
	local x = pos.x // GRID_SIZE; -- x
	local y = pos.z // GRID_SIZE; -- y
	return x * X_POWER + y;
end

--NOTE: this function not support cocurrent
local grid_list = {}
local function grid_around(i)
	local g, xi, yi, cnt
	i = i - X_OFFSET --x - 1, y
	cnt = 0
	for j = 0, 2 do
		-- x - 1 + i, y
		i = i + X_OFFSET
		l = GRID[i]
		if l then
			cnt = cnt + 1
			grid_list[cnt] = l
		end
		-- x - 1, y - 1
		local row = i- Y_OFFSET
		l = GRID[row]
		if l then
			cnt = cnt + 1
			grid_list[cnt] = l
		end
		--x - 1 + i, y + 1
		row = i + Y_OFFSET
		l = GRID[row]
		if l then
			cnt = cnt + 1
			grid_list[cnt] = l
		end
	end
	grid_list.count = cnt
	return grid_list
end

--NOTE:every function use the result of grid_around_uids
--must clear it, if not, please pass a empty table
local around_uids = {}
local function grid_around_uids(gridi, tbl)
	local count = 0
	local uids
	if tbl then
		uids = tbl
	else
		uids = around_uids
	end
	local list = grid_around(gridi)
	for i = 1, list.count do
		local l = list[i]
		for uid, _ in pairs(l) do
			uids[uid] = true
			count = count + 1
		end
	end
	return uids, count
end

local function debug_grid_around(gridi)
	local l = grid_around_uids(gridi)
	print("debug_gridi-----", gridi)
	for uid, _ in pairs(l) do
		print(uid)
	end
	print("debug_gridi====")
end

local function multicast(gridi, cmd, ack, except)
	--TODO:optimise it by array
	local list, count = grid_around_uids(gridi)
	if count == 0 then
		return
	end
	channel.multicast(list, cmd, ack)
end

local ack_grab = {
	players = false
}

local function r_grab(uid, req, gateid)
	local p = {}
	ack_grab.players = p
	local i = grid_index(req.pos)
	print("r_grab uid:", uid, "grid:", i)
	local uids = grid_around_uids(i)
	for uid, _ in pairs(uids) do
		p[uid] = {
			uid = uid,
			pos = PLAYER[uid].pos
		}
		uids[uid] = nil
		print("grab uid has", uid)
	end
	channel.sendgate(gateid, uid, "a_grab", ack_grab)
end


local ack_enter = {
	uid = false,
	pos = false,
}

local function r_enter(uid, req, gateid)
	local pos = req.pos
	local i = grid_index(pos)
	print("before", GRID[i])
	debug_grid_around(i)
	local p = PLAYER[uid]
	if not p then
		p = {
			uid = uid,
			pos = pos,
			grid = i,
		}
		PLAYER[uid] = p
	else
		p.pos = pos
		GRID[p.grid][uid] = nil
		p.grid = i
	end
	local g = GRID[i]
	if not g then
		g = {}
		GRID[i] = g
	end
	g[uid] = true
	print("after", GRID[i])
	debug_grid_around(i)

	ack_enter.uid = uid
	ack_enter.pos = pos
	channel.attach(uid, gateid)
	print("r_enter uid:", uid, "grid", i)
	multicast(i, "a_enter", ack_enter, 0)
end

local ack_move = {
	uid = false,
	pos = false,
	rot = false,
}
local function r_move(uid, req)
	local pos = req.pos
	local p = assert(PLAYER[uid])
	--print("r_move", uid, p.grid, GRID[p.grid], uid)
	local i = grid_index(pos)
	if i ~= p.grid then
		GRID[p.grid][uid] = nil
		p.grid = i
		local g = GRID[i]
		if not g then
			g = {}
			GRID[i] = g
		end
		g[uid] = true
		ack_enter.uid = uid
		ack_enter.pos = pos
		multicast(i, "a_enter", ack_enter, uid)
	end
	ack_move.uid = uid
	ack_move.pos = req.pos
	ack_move.rot = req.rot
	multicast(i, "a_move", ack_move, uid)
end

local ack_leave = {
	uid = false
}

local function r_leave(uid, req, gateid)
	local p = assert(PLAYER[uid], uid)
	local i = p.GRID
	GRID[i][uid] = nil
	PLAYER[uid] = nil
	ack_leave.uid = uid
	multicast(i, "a_leave", ack_enter, uid)
end

channel.reg_client("r_grab", r_grab)
channel.reg_client("r_enter", r_enter)
channel.reg_client("r_move", r_move)
channel.reg_client("r_leave", r_leave)

function M.start()
	return true
end

local function s_logout(uid, _)
	print("s_logout", uid)
	local p = PLAYER[uid]
	if p then
		PLAYER[uid] = nil
		GRID[p.grid][uid] = nil
	end
	channel.detach(uid)
end

local sr_sceneonline = {
	rpc = false,
	sceneid = SCENEID,
}
local sr_locateplayer = {
	rpc = false,
	uids = false,
}
local function e_connect(gate)
	local ack = gate:call("sr_sceneonline", sr_sceneonline)
	if not ack then
		print("[scene] sr_sceneonline fail on gate:", gate.gateid)
		return
	end
	for _, v in pairs(ack.uids) do
		print("[scene] e_connect online uid", v)
	end
	sr_locateplayer.uids = ack.uids
	ack = gate:call("sr_locateplayer", sr_locateplayer)
	if not ack then
		print("[scene] sr_locateplayer fail on gate:", gate.gateid)
		return
	end
	for _, v in pairs(ack.players) do
		print("[scene] e_connect locate uid", v.uid, v.pos, v.rot)
	end
end

channel.reg_server("s_logout", s_logout)
channel.hook_connect(e_connect)

return M
