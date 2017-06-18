using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using zprotobuf;
using client_zproto;

public class GameScene: MonoBehaviour {
	//component
	//
	private ThirdPersonController controller;
	private ThirdPerson mainPlayer;

	void ComponentStart() {
		CameraManager.main = Camera.main;
		controller = GetComponent<ThirdPersonController>();
		int uid = Player.Instance.Uid;
		Vector2 pos = Player.Instance.Pos;
		mainPlayer = ThirdPersonManager.Instance.CreateCharacter(uid, pos);
		controller.Attach(mainPlayer);
		Debug.Log("GameScene MainPlayer:" + pos);
	}
	void ProtocolStart() {
		a_grab @a_grab = new a_grab();
		a_enter @a_enter = new a_enter();
		a_leave @a_leave = new a_leave();
		NetInstance.Gate.Register(@a_grab, ack_grab);
		NetInstance.Gate.Register(@a_enter, ack_enter);
		NetInstance.Gate.Register(@a_leave, ack_leave);

		var pos = Player.Instance.Pos;
		r_enter @r_enter = new r_enter();
		@r_enter.pos = new vector2();
		Tool.ToProto(ref @r_enter.pos, pos);
		NetInstance.Gate.Send(@r_enter);
		Debug.Log("r_enter");
	}

	void Start () {
		ComponentStart();
		ProtocolStart();
		return ;
	}

	///protocol interface
	void ack_grab(int err, wire obj) {
		Debug.Log("Grab ack");
		a_grab ack = (a_grab)obj;
		Vector2 pos = new Vector2();
		for (int i = 0; i < ack.players.Length; i++) {
			var p = ack.players[i];
			Tool.ToNative(ref pos, p.pos);
			Debug.Log("[Net]Grab:" + p.uid);
			ThirdPersonManager.Instance.CreateCharacter(p.uid, pos);
		}
	}

	void ack_enter(int err, wire obj) {
		Debug.Log("a_enter:" + err);
		if (err != 0)
			return ;
		a_enter ack = (a_enter)obj;
		Debug.Log("[Net]a_enter uid:" + ack.uid + "PlayerUid:" + mainPlayer.Uid);
		if (ack.uid != Player.Instance.Uid) {
			Vector2 pos = new Vector2();
			Debug.Log("ack.pos" + ack.pos + ":" + ack.uid);
			Tool.ToNative(ref pos, ack.pos);
			ThirdPersonManager.Instance.CreateCharacter(ack.uid, pos);
			return;
		}
		//grab
		r_grab @r_grab = new r_grab();
		@r_grab.pos = new vector2();
		Tool.ToProto(ref @r_grab.pos, mainPlayer.transform.position);
		NetInstance.Gate.Send(@r_grab);
		Debug.Log("r_grab");
	}

	void ack_leave(int err, wire obj) {
		Debug.Log("Leave");

	}

	/////unity interface
	void Update () {

	}

}
