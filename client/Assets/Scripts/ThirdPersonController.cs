using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using client_zproto;

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
	}

	// Update is called once per frame
	void Update () {

	}

	void FixedUpdate() {
		if (player == null) {
			int uid = Player.Instance.Uid;
			player = ThirdPersonManager.Instance.GetCharacter(uid);
			return ;
		}
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

		r_sync sync = new r_sync();
		sync.pos = new vector3();
		sync.rot = new rotation();
		sync.pos.x = (int)(player.transform.position.x * 10000);
		sync.pos.y = (int)(player.transform.position.y * 10000);
		sync.pos.z = (int)(player.transform.position.z * 10000);
		sync.rot.x = (int)(player.transform.rotation.x * 10000);
		sync.rot.y = (int)(player.transform.rotation.y * 10000);
		sync.rot.z = (int)(player.transform.rotation.z * 10000);
		sync.rot.w = (int)(player.transform.rotation.w * 10000);
	}
}
