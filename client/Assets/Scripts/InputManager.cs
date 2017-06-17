using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class InputManager : MonoBehaviour {

	public static float GetMoveX() {
		return CrossPlatformInputManager.GetAxis("Horizontal");
	}
	public static float GetMoveZ() {
		return CrossPlatformInputManager.GetAxis("Vertical");
	}
	public static float GetTurnX() {
		return CrossPlatformInputManager.GetAxis("Mouse Y");
	}
	public static float GetTurnY() {
		return CrossPlatformInputManager.GetAxis("Mouse X");
	}
	public static bool GetFire1() {
		return CrossPlatformInputManager.GetButtonDown("Fire1");
	}
	public static bool GetSwap() {
		return CrossPlatformInputManager.GetButtonDown("Fire2");
	}
	public static bool GetJump() {
		return CrossPlatformInputManager.GetButtonDown("Jump");
	}
}
