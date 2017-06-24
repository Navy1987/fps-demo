using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class JoyStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
	public GameObject joy_main;
	public GameObject joy_move;

	private const int INVALID = -100;
	private int _ID = INVALID;
	private Vector2 control = Vector2.zero;
	public void OnPointerDown(PointerEventData data) {
		_ID = data.pointerId;
		GameInstance.console.Log("OnPointerDown:" + data.pointerId);
	}

	public void OnPointerUp(PointerEventData data) {
		GameInstance.console.Log("OnPointerUp:" + data.pointerId);
		if (data.pointerId != _ID)
			return ;
		_ID = INVALID;
		joy_move.transform.localPosition = Vector3.zero;
		control = Vector2.zero;
	}

	void Process(Vector3 mousePos3) {
		var c = CameraManager.ui;
		mousePos3.z = 0.0f;
		mousePos3 = c.ScreenToWorldPoint(mousePos3);
		var move = mousePos3- joy_main.transform.position;
		control = new Vector2(move.x, move.y);
		control = Vector2.ClampMagnitude(control, 20.0f);
		joy_move.transform.localPosition = control;
		GameInstance.console.Log("OnPointerUp:" + control);
	}

	public Vector2 Move {
		get {
			return control;
		}
	}

	void Awake() {
		GameInstance.joystick = this;
	}

#if UNITY_EDITOR
	void Update () {
		if (_ID == INVALID)
			return ;
		Process(Input.mousePosition);
	}
#else
	void Update() {
		if (_ID == INVALID)
			return ;
		if (Input.touchCount < _ID + 1)
			return ;
		var touch = Input.touches[_ID].position;
		Process(touch);
	}
#endif
}
