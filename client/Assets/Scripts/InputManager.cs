using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {
	public static float GetAxis(string axisName) {
		return Input.GetAxis(axisName);
	}
}
