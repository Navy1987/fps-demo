local crypt = require "crypt"
local log = require "log"
local db = require "db"
local session = require "session"
local channel = require "channel"
local errno = require "protocol.errno"

local challenge_key = {}
local function challenge_close(fd)
	challenge_key[fd] = nil
end

local function create(fd, req)
	local uid = db.account_id(req.user)
	if not uid then
		uid = db.account_create(req.user, req.passwd)
	end
	uid = tonumber(uid)
	local ack = {
		uid = uid
	}
	channel.send(fd, "a_create", ack)
	log.print("[account] create user:", req.user, "uid:", uid, "passwd", req.passwd)
end

local function challenge(fd, req)
	local key = crypt.randomkey()
	local ack = {
		randomkey = key
	}
	challenge_key[fd] = key
	channel.hookclose(fd, challenge_close)
	channel.send(fd, "a_challenge", ack)
	log.print("[account] challenge fd:", fd, key)
end

local function auth(fd, req)
	local key = challenge_key[fd]
	if not key then
		return channel.error(fd, "a_login", errno.ACCOUNT_NO_CHALLENGE)
	end
	challenge_key[fd] = nil
	channel.hookclose(fd, nil)
	local passwd, uid = db.account_passwd(req.user)
	if not passwd or not uid then
		return channel.error(fd, "a_login", errno.ACCOUNT_NO_USER)
	end
	local hmac = crypt.hmac(passwd, key)
	if hmac ~= req.passwd then
		return channel.error(fd, "a_login", errno.ACCOUNT_NO_PASSWORD);
	end
	uid = tonumber(uid)
	local ack = {
		uid = uid,
		token = session.auth_token(uid)
	}
	channel.send(fd, "a_login", ack);
end

channel.reg_client("r_create", create)
channel.reg_client("r_challenge", challenge)
channel.reg_client("r_auth", auth)

