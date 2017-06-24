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

	void Control(Vector3 mousePos3) {
		var c = CameraManager.ui;
		mousePos3.z = 0.0f;
		mousePos3 = c.ScreenToWorldPoint(mousePos3);
		var move = mousePos3- joy_main.transform.position;
		control = new Vector2(move.x, move.y);
		control = Vector2.ClampMagnitude(control, 20.0f);
		joy_move.transform.localPosition = control;
		GameInstance.console.Log("OnPointerUp:" + control);
	}

#if UNITY_EDITOR
	void Update () {
		if (_ID == INVALID)
			return ;
		Control(Input.mousePosition);
	}
#else
	void Update() {
		if (_ID == INVALID)
			return ;
		if (Input.touchCount < _ID + 1)
			return ;
		var touch = Input.touches[_ID].position;
		Control(touch);
	}
#endif
/*
	if (!m_Dragging)
			{
				return;
			}
			if (Input.touchCount >= m_Id + 1 && m_Id != -1)
			{
#if !UNITY_EDITOR

            if (controlStyle == ControlStyle.Swipe)
            {
                m_Center = m_PreviousTouchPos;
                m_PreviousTouchPos = Input.touches[m_Id].position;
            }
            Vector2 pointerDelta = new Vector2(Input.touches[m_Id].position.x - m_Center.x , Input.touches[m_Id].position.y - m_Center.y).normalized;
            pointerDelta.x *= Xsensitivity;
            pointerDelta.y *= Ysensitivity;
#else
				Vector2 pointerDelta;
				pointerDelta.x = Input.mousePosition.x - m_PreviousMouse.x;
				pointerDelta.y = Input.mousePosition.y - m_PreviousMouse.y;
				m_PreviousMouse = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f);
#endif
				UpdateVirtualAxes(new Vector3(pointerDelta.x, pointerDelta.y, 0));
			}
		}
	}
*/
}
