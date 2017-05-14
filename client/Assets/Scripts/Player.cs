using System.Collections;
using System.Collections.Generic;

public class Player {
	private static Player inst = null;
	private int uid = 0;
	public static Player Instance {
		get {
			if (inst == null)
				inst = new Player();
			return inst;
		}
	}

	public void Init(int uid) {
		this.uid = uid;
		return ;
	}

	public int Uid {
		get {
			return uid;
		}
	}
}
