using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : MonoBehaviour {
	public bool single = false;
	//control
	public float run_speed = 5.0f;
	public float turn_X_speed = 10.0f;
	public float turn_Y_speed = 6.0f;
	public float weaponRange = 50f;


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
