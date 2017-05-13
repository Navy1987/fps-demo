local crypt = require "crypt"
local server = require "server"
local log = require "log"
local db = require "db"
local router = require "router"
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
	server.send(fd, "a_create", ack)
	log.print("[account] create user:", req.user, "uid:", uid, "passwd", req.passwd)
end

local function challenge(fd, req)
	local key = crypt.randomkey()
	local ack = {
		randomkey = key
	}
	challenge_key[fd] = key
	server.hookclose(fd, challenge_close)
	server.send(fd, "a_challenge", ack)
	log.print("[account] challenge fd:", fd, key)
end

local function login(fd, req)
	local key = challenge_key[fd]
	if not key then
		return server.error(fd, "a_login", errno.ACCOUNT_NO_CHALLENGE)
	end
	challenge_key[fd] = nil
	server.hookclose(fd, nil)
	local passwd, uid = db.account_passwd(req.user)
	if not passwd or not uid then
		return server.error(fd, "a_login", errno.ACCOUNT_NO_USER)
	end
	local hmac = crypt.hmac(passwd, key)
	if hmac ~= req.passwd then
		return server.error(fd, "a_login", errno.ACCOUNT_NO_PASSWORD);
	end
	local ack = {
		uid = tonumber(uid),
	}
	server.send(fd, "a_login", ack);
end

router.reg("r_create", create)
router.reg("r_challenge", challenge)
router.reg("r_login", login)

