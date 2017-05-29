using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using client_zproto;

public class ThirdPersonController : MonoBehaviour {
	//component
	private LineRenderer aimLine;

	//debug
	public Text display;
	public Camera playercamera;

	private float delta = 0;
	private ThirdPerson player;

	// Use this for initialization
	void Start () {
		aimLine = GetComponent<LineRenderer>();
	}

	// Update is called once per frame
	void Update () {

	}

	void PlayerSync() {
		if (player == null)
			return ;
		float turn_X_speed = GameConfig.Instance.turn_X_speed;
		float turn_Y_speed = GameConfig.Instance.turn_Y_speed;
		float run_speed = GameConfig.Instance.run_speed;
		//character rotation
		Quaternion rot = player.transform.localRotation;
		float mouseY = turn_Y_speed * InputManager.GetTurnY();
		float mouseX = turn_X_speed * InputManager.GetTurnX()  * Time.deltaTime;
		Quaternion rotY = Quaternion.Euler(0.0f, mouseY, 0.0f);
		Quaternion rotX = Quaternion.Euler(-mouseX, 0.0f, 0.0f);
		rot = rot * rotY * rotX;
		//position
		float moveX = InputManager.GetMoveX();
		float moveZ = InputManager.GetMoveZ();
		Vector3 move = new Vector3(moveX, 0, moveZ);
		display.text = "x:" + move.x + " y:" + move.y + " z:" + move.z;
		Vector3 forward = move * Time.deltaTime * run_speed * (0.1f / Time.deltaTime);
		forward = rot * forward;
		Vector3 pos = player.transform.position;
		pos += forward;
		//sync
		player.Sync(pos, rot);
	}

	void PlayerFire() {

	}

	void FixedUpdate() {
		if (player == null)
			return ;
		PlayerFire();
		delta += Time.deltaTime;
		if (delta < 0.1f)
			return ;
		delta -= 0.1f;
		PlayerSync();
		if (!GameConfig.Instance.single)
			ThirdPersonManager.Instance.SyncCharacter(Player.Instance.Uid);
	}

	public void Attach(ThirdPerson p) {
		player = p;
		p.LookAt(playercamera);
	}
}
