local zproto = require "zproto"
local proto = zproto:load("protocol/client.zproto")
assert(proto)
return proto


