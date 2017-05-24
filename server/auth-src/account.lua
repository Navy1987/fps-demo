local core = require "silly.core"
local crypt = require "crypt"
local log = require "log"
local db = require "db"
local const = require "const"
local channel = require "virtualsocket.channel"
local errno = require "protocol.errno"

local challenge_key = {}
local connect_stamp = {}
local connect_time = {}
local online_uid = {}

local auth_ok = {
	uid = false,
	stamp = false
}

local function challenge_close(fd)
	challenge_key[fd] = nil
end

local function auth_pass(fd, uid, gateid)
	auth_ok.uid = uid
	auth_ok.stamp = connect_stamp[fd]
	print("auth_pass", fd, auth_ok.stamp)
	local last_gate = online_uid[uid]
	if last_gate then
		print("kick user", uid, last_gate)
		channel.kickgate(last_gate, uid)
	end
	online_uid[uid] = gateid
	channel.sendservergate(gateid, fd, "s_authok", auth_ok)
	connect_stamp[fd] = nil
	connect_time[fd] = nil
end

local function authstamp(fd, req)
	print("auth stamp", req.stamp)
	connect_stamp[fd] = req.stamp
	connect_time[fd] = core.current()
end

local function clean_timer()
	local current = core.current()
	for k, v in pairs(connect_time) do
		if (current - v) > 3000 then
			connect_time[k] = nil
			connect_stamp[k] = nil
			challenge_key[k] = nil
		end
	end
	core.timeout(1000, clean_timer)
end

core.timeout(1000, clean_timer)

local function create(fd, req, gateid)
	local uid = db.account_id(req.user)
	if not uid then
		uid = db.account_create(req.user, req.passwd)
	end
	uid = tonumber(uid)
	local ack = {
		uid = uid
	}
	channel.sendgate(gateid, fd, "a_create", ack)
	log.print("[account] create user:", req.user, "uid:", uid, "passwd", req.passwd)
end

local function challenge(fd, req, gateid)
	local key = crypt.randomkey()
	local ack = {
		randomkey = key
	}
	challenge_key[fd] = key
	channel.sendgate(gateid, fd, "a_challenge", ack)
	log.print("[account] challenge fd:", fd, key)
end

local function login(fd, req, gateid)
	local key = challenge_key[fd]
	if not key then
		return channel.errorgate(gateid, fd, "a_login", errno.ACCOUNT_NO_CHALLENGE)
	end
	challenge_key[fd] = nil
	local passwd, uid = db.account_passwd(req.user)
	if not passwd or not uid then
		return channel.errorgate(gateid, fd, "a_login", errno.ACCOUNT_NO_USER)
	end
	local hmac = crypt.hmac(passwd, key)
	if hmac ~= req.passwd then
		return channel.errorgate(gateid, fd, "a_login", errno.ACCOUNT_NO_PASSWORD);
	end
	uid = tonumber(uid)
	local ack = {
		uid = uid,
	}
	auth_pass(fd, uid, gateid)
	channel.sendgate(gateid, fd, "a_login", ack);
end

local function offline(uid, gateid)
	online_uid[uid] = nil
end

channel.reg_server("s_authstamp", authstamp)
channel.reg_client("r_create", create)
channel.reg_client("r_challenge", challenge)
channel.reg_client("r_login", login)

