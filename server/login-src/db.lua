local core = require "silly.core"
local env = require "silly.env"
local redis = require "redis"

local inst
local db = {}

function db.start()
	local err
	inst, err = redis:connect {
		addr = env.get("db_port")
	}
	print("Redis Connect:", inst, err)
	return not not inst
end

--[[
account:userid
account:[uid]:name
account:[uid]:passwd
account:[uid]:nick
]]
local account_idx = "account:idx"
local account_userid = "account:userid"
local account_name = "account:%s:name"
local account_passwd = "account:%s:passwd"
local account_nick = "account:%s:nick"

function db.account_id(user)
	local ok, err = inst:hget(account_userid, user)
	if not ok then
		return nil
	end
	return err
end

function db.account_create(user, passwd)
	local ok, id = inst:incr(account_idx)
	assert(ok, id)
	local key = string.format(account_name, id)
	inst:set(key, user)
	key = string.format(account_passwd, id)
	inst:set(key, passwd)
	key = string.format(account_nick, id)
	inst:set(key, "大侠")
	inst:hset(account_userid, user, id)
	return id
end

function db.account_passwd(user)
	assert(user)
	local ok, id = inst:hget(account_userid, user)
	if not ok then
		return nil
	end
	local key = string.format(account_passwd, id)
	local ok, passwd = inst:get(key)
	assert(ok, passwd)
	return passwd, id
end

return db

