using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using client_zproto;

public class ThirdPersonController : MonoBehaviour {
	//component
	public Camera playercamera;
	public float turn_speed = 1.0f;
	public float run_speed = 1.0f;
	//debug
	public Text display;

	private float delta = 0;
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

	void SyncPlayer() {
		if (player == null)
			return ;
		float V = InputManager.GetAxis("Vertical");
		float H = InputManager.GetAxis("Horizontal");
		Vector3 move = new Vector3(H, 0, V);
		display.text = "x:" + move.x + " y:" + move.y + " z:" + move.z;
		//rotation
		Quaternion rot = player.transform.rotation;
		float mouseX = turn_speed * Input.GetAxis("Mouse X");
		Quaternion cook = Quaternion.Euler(0, mouseX, 0);
		rot = cook * rot;;
		//position
		Vector3 forward = new Vector3(H, 0, V) * Time.deltaTime * run_speed;
		forward = rot * forward;
		Vector3 pos = player.transform.position;
		pos += forward;
		//sync
		player.Sync(pos, rot);
	}

	void FixedUpdate() {
		if (player == null) {
			int uid = Player.Instance.Uid;
			player = ThirdPersonManager.Instance.GetCharacter(uid);
			//player = ThirdPersonManager.Instance.CreateCharacter(uid);
			return ;
		}
		SyncPlayer();
		var pos = player.transform.position;
		var rot = player.transform.rotation;
		playercamera.transform.position = pos;
		playercamera.transform.rotation = rot * camera_rot;
		playercamera.transform.position += playercamera.transform.rotation * camera_pos;
		delta += Time.deltaTime;
		if (delta < 0.1f)
			return ;
		delta -= 0.1f;
		ThirdPersonManager.Instance.SyncCharacter(Player.Instance.Uid);
	}
}
