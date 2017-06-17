using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using client_zproto;

public class ThirdPersonController : MonoBehaviour {
	//debug
	public Text player_control;
	public Text player_coord;
	public Camera maincamera;

	private float delta = 0;
	private ThirdPerson player;
	private WorldMap worldmap;
	private CameraFollow follow = new CameraFollow();

	void MoveController() {
		if (player == null)
			return ;
		delta += Time.deltaTime;
		if (delta < 0.1f)
			return ;
		delta -= 0.1f;
		Vector3 move_pos;
		Quaternion move_rot;
		bool move_jump = false;
		bool move_crouch = false;
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
		player_control.text = "input(x:" + move.x.ToString("F1") + ",z:" + move.z.ToString("F1") + ")";
		Vector3 forward = move * Time.deltaTime * run_speed * (0.1f / Time.deltaTime);
		forward = rot * forward;
		move_pos = player.transform.position + forward;
		//control player
		var srot = player.Shadow.rot;
		var angleX = srot.eulerAngles.x - mouseX;
		Quaternion rotX = Quaternion.Euler(angleX, 0.0f, 0.0f);
		rot = rot * rotX;
		move_rot = Tool.ClampRotationAroundXAxis(rot);
		//jump crouch
		move_jump = InputManager.GetJump();
		player.MoveTo(move_pos, move_rot, move_jump, move_crouch);
		//synchroize
		if (!GameConfig.Instance.single)
			ThirdPersonManager.Instance.SyncCharacter(Player.Instance.Uid);
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
				Debug.Log("Hit Part:" + hitTag + " of " + personObj.tag +
						"[" + person.Uid + "]" +
						" Hp:" + hurt + " Shoot:" + hitPoint +
						" Delta:" + (hitPoint - personObj.transform.position));
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

	void PlayerPos() {
		var pos = player.transform.position;
		var sx = pos.x.ToString("F1");
		var sz = pos.z.ToString("F1");
		player_coord.text = "coord(x:" + sx + ",z:" + sz + ")";
	}
	////////////interface
	public void Attach(ThirdPerson p) {
		player = p;
		follow.Follow(p);
		worldmap.UpdatePosition(p.transform.position);
	}

	////////////inherit
	void Awake() {
		worldmap = GetComponent<WorldMap>();
		Debug.Log("Awake");
	}
	void Start() {
		CameraManager.main = maincamera;
		follow.Start();
		Debug.Log("[Controller] Follow" + follow);
	}

	void FixedUpdate() {
		if (player == null)
			return ;
		PlayerFire();
		PlayerSwap();
		MoveController();
		PlayerPos();
	}

	void LateUpdate() {
		follow.LateUpdate();
		worldmap.UpdatePosition(player.transform.position);
	}
}
