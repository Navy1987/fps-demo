using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public static float GetMoveX() {
		return Input.GetAxis("Horizontal");
	}
	public static float GetMoveZ() {
		return Input.GetAxis("Vertical");
	}
	public static float GetTurnX() {
		return Input.GetAxis("Mouse Y");
	}
	public static float GetTurnY() {
		return Input.GetAxis("Mouse X");
	}
	public static bool GetFire1() {
		return Input.GetButtonDown("Fire1");
	}
	public static bool GetSwap() {
		return Input.GetButtonDown("Fire2");
	}
	public static bool GetJump() {
		return Input.GetButtonDown("Jump");
	}
}
