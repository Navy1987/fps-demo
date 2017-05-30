using System.Collections;
using System.Collections.Generic;

public class PartHurt {
	private static PartHurt inst = null;
	private Dictionary<string, int> hurt_table = new Dictionary<string, int>();
	private PartHurt() {
		hurt_table["HitHead"] = 1000;
		hurt_table["HitBack"] = 500;
	}
	public static PartHurt Instance {
		get {
			if (inst == null)
				inst = new PartHurt();
			return inst;
		}
	}
	public int GetHurt(string part) {
		if (hurt_table.ContainsKey(part))
			return hurt_table[part];
		return 10;
	}
}
