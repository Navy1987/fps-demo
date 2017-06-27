using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager : MonoBehaviour {

	public static float GetMoveX() {
		return 0.0f;
	}
	public static float GetMoveZ() {
#if UNITY_EDITOR
		return Input.GetAxis("Vertical") * 8 * Time.deltaTime;
#else
		return GameInstance.joystick.Move.y * Time.deltaTime;
#endif
	}
	public static float GetTurnX() {
		return 0.0f;
	}
	public static float GetTurnY() {
#if UNITY_EDITOR
		float h = Input.GetAxis("Horizontal");
		if (h < 0.1f && h >-0.1f)
			return 0.0f;
		float v = Input.GetAxis("Vertical");
		float t = Mathf.Atan2(h, v) * Time.deltaTime * 20;
		return t;
#else
		Vector2 move = GameInstance.joystick.Move;
		return Mathf.Atan2(move.x, move.y);
#endif
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
