local M = {}

function M.hex(str)
	local tbl = {}
	for i = 1, #str do
		table.insert(tbl, string.format("%02x", str:byte(i)))
	end
	print(table.concat(tbl, " "))
end

return M

