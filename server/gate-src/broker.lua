local M = {}

local serverid_brokerfd = {
	--['typ'] = {
	--	['sceneid'] = 'broker_fd'
	--}
}

local brokerfd_type = {
	--['broker_fd'] = typ
}

local cmd_type = {
	--['cmd'] = 'type'
}

function M.register(broker_fd, id, typ, cmd_list)
	for _, cmd in pairs(cmd_list) do
		cmd_type[cmd] = typ
		print("[gate-broker] register cmd", string.format("0x%02x", cmd),
			typ, id, broker_fd)
	end
	local s = serverid_brokerfd[typ]
	if not s then
		s = {}
		serverid_brokerfd[typ] = s
	end
	s[id] = broker_fd
	brokerfd_type[broker_fd] = typ
end

function M.clear(broker_fd)
	local typ = brokerfd_type[broker_fd]
	assert(typ, broker_fd)
	local l = serverid_brokerfd[typ]
	for k, v in pairs(l) do
		if v == broker_fd then
			l[k] = nil
		end
	end
end

function M.getbroker(agent, cmd)
	local typ = assert(cmd_type[cmd], cmd)
	local id
	if typ == "role" then
		id = agent.roleserver
	elseif typ == "scene" then
		id = agent.sceneserver
	end
	local l = serverid_brokerfd[typ]
	return l[id]
end

local SCENE = "scene"

function M.getscene(sceneid)
	return serverid_brokerfd[SCENE][sceneid]
end

return M

