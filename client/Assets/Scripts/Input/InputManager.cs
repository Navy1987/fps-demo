using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public static float GetMoveX() {
		return 0.0f;
	}
	public static float GetMoveZ() {
		return GameInstance.joystick.Move.y * Time.deltaTime;
	}
	public static float GetTurnX() {
		return 0.0f;
	}
	public static float GetTurnY() {
		Vector2 move = GameInstance.joystick.Move;
		return Mathf.Atan2(move.x, move.y);
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
