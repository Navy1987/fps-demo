local errno = {

ACCOUNT_NO_CHALLENGE = 1,	--挑战信息失败
ACCOUNT_NO_USER = 2,		--用户不存在
ACCOUNT_NO_PASSWORD = 3,	--密码不对
ACCOUNT_NO_GATEID = 4,		--网关不存在
ACCOUNT_SESSION_TIMEOUT = 5,	--获取登录session超时
ACCOUNT_KICK_TIMEOUT = 6,	--踢除已有玩家超时

}

return errno

