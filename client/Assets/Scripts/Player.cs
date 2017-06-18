using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player {
	private static Player inst = null;
	private int uid = 0;
	private Vector2 pos;
	public static Player Instance {
		get {
			if (inst == null)
				inst = new Player();
			return inst;
		}
	}

	public void Init(int uid, Vector2 pos) {
		this.uid = uid;
		this.pos = pos;
		return ;
	}

	public int Uid {
		get {
			return uid;
		}
	}
	public Vector2 Pos {
		get {
			return pos;
		}
	}
}
