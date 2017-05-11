using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirdPersonController : MonoBehaviour {
	//component
	public Camera playercamera;
	//debug
	public Text display;

	private ThirdPerson player;
	private Vector3 camera_pos;
	private Quaternion camera_rot;

	// Use this for initialization
	void Start () {
		camera_pos = playercamera.transform.position;
		camera_rot = playercamera.transform.rotation;
		Debug.Log("PlayerMgr:" + ThirdPersonManager.Instance);
		player = ThirdPersonManager.Instance.CreateCharacter(1000);
	}

	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate() {
		float V = InputManager.GetAxis("Vertical");
		float H = InputManager.GetAxis("Horizontal");
		Vector3 m = new Vector3(H, 0, V);
		display.text = "x:" + m.x + " y:" + m.y + " z:" + m.z;
		player.Move(m, false, false);
		var pos = player.transform.position;
		var rot = player.transform.rotation;
		playercamera.transform.position = pos;
		playercamera.transform.rotation = rot * camera_rot;
		playercamera.transform.position += playercamera.transform.rotation * camera_pos;
	}
}
