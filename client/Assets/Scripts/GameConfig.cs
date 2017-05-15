using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour {
	public bool single = false;
	public float run_speed = 1.0f;

	private static GameConfig inst = null;
	void Start () {

	}

	void Update () {

	}

	public static GameConfig Instance {
		get {
			if (inst == null)
				inst = GameObject.FindObjectOfType<GameConfig>();
			return inst;
		}
	}
}
