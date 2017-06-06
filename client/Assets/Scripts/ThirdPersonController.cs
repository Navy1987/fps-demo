using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using client_zproto;

public class ThirdPersonController : MonoBehaviour {
	//debug
	public Text display;
	public Camera maincamera;

	private float delta = 0;
	private ThirdPerson player;
	private CameraFollow follow = new CameraFollow();

	// Use this for initialization
	void Start() {
		CameraManager.main = maincamera;
		follow.Start();
		Debug.Log("[Controller] Follow" + follow);
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
		float mouseX = turn_X_speed * InputManager.GetTurnX();
		Quaternion rotY = Quaternion.Euler(0.0f, mouseY, 0.0f);
		rot = rot * rotY;
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
		var srot = player.Shadow.rot;
		var angleX = srot.eulerAngles.x - mouseX;
		Quaternion rotX = Quaternion.Euler(angleX, 0.0f, 0.0f);
		rot = rot * rotX;
		rot = Tool.ClampRotationAroundXAxis(rot);
		player.Sync(pos, rot);
	}

	void PlayerFire() {
		if (!InputManager.GetFire1())
			return;
		RaycastHit hit;
		Vector3 rayOrigin = maincamera.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0.0f));
		Debug.Log("RayOrigin:" + rayOrigin);
		if (Physics.Raycast(rayOrigin, maincamera.transform.forward, out hit, GameConfig.Instance.weaponRange, 1)) {
			var hitPoint = hit.point;
			var hitObj = hit.collider.gameObject;
			var hitTag = hitObj.tag;
			Debug.Log("Hit Tag:" + hitObj.tag);
			if (hitObj.tag == "Untagged") {
				return ;
			}
			if (Physics.Raycast(rayOrigin, maincamera.transform.forward, out hit, GameConfig.Instance.weaponRange, 1 << 9)) {
				var personObj = hit.collider.gameObject;
				var person = personObj.GetComponent<ThirdPerson>();
				var hurt = PartHurt.Instance.GetHurt(hitTag);
				Debug.Log("Hit Part:" + hitTag + " of " + personObj.tag + "[" + person.Uid + "]" + " Hp:" + hurt + " Shoot:" + hitPoint + " Delta:" + (hitPoint - personObj.transform.position));
				hitPoint -= personObj.transform.position;
				ThirdPersonManager.Instance.Shoot(player.Uid, person.Uid, hitPoint);
			}
		}
	}

	void PlayerSwap() {
		if (!InputManager.GetSwap())
			return ;
		Debug.Log("Swap");
		player.SwapWeapon();
	}

	void PlayerJump() {
		if (!InputManager.GetJump())
			return ;
		player.Jump();

	}

	public void Attach(ThirdPerson p) {
		player = p;
		follow.Follow(p);
	}

	void FixedUpdate() {
		if (player == null)
			return ;
		PlayerFire();
		PlayerSwap();
		PlayerJump();
		delta += Time.deltaTime;
		if (delta < 0.1f)
			return ;
		delta -= 0.1f;
		PlayerSync();
		if (!GameConfig.Instance.single)
			ThirdPersonManager.Instance.SyncCharacter(Player.Instance.Uid);
	}

	void LateUpdate() {
		follow.LateUpdate();
	}

}
