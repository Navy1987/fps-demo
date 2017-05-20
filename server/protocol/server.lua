local zproto = require "zproto"
local proto = zproto:load("protocol/server.zproto")
assert(proto)
return proto


