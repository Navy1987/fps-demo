﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using zprotobuf;
using client_zproto;

public class GameWnd : MonoBehaviour {
	public Button enter_btn;
	public Button leave_btn;

	// Use this for initialization
	void Start () {
		//event
		enter_btn.onClick.AddListener(on_enter);
		leave_btn.onClick.AddListener(on_leave);

		//socket
		a_join join = new a_join();
		a_battleinfo battle = new a_battleinfo();
		NetInstance.Gate.Register(join, ack_join);
		NetInstance.Gate.Register(battle, ack_battle);
	}

	// Update is called once per frame
	void Update () {

	}

	void on_enter() {
		Debug.Log("Enter");
		r_join req = new r_join();
		req.join = 1;
		NetInstance.Gate.Send(req);
	}

	void on_leave() {
		Debug.Log("Leave");
		r_join req = new r_join();
		req.join = 0;
		NetInstance.Gate.Send(req);
	}

	///protocol
	void ack_join(int err, wire obj) {
		a_join ack = (a_join) obj;
		Debug.Log("Ack_Join:" +  err + ":" + ack.uid + ":" + ack.join);
		if (ack.join == 1) {
			ThirdPersonManager.Instance.CreateCharacter(ack.uid);
			r_battleinfo req = new r_battleinfo();
			NetInstance.Gate.Send(req);
		} else {
			ThirdPersonManager.Instance.DeleteCharacter(ack.uid);
		}
	}

	void ack_battle(int err, wire obj) {
		a_battleinfo ack = (a_battleinfo)obj;
		for (int i = 0; i < ack.uid.Length; i++) {
			int uid = ack.uid[i];
			Debug.Log("[GameWnd] BattleInfo CreateCharacter:" + uid);
			ThirdPersonManager.Instance.CreateCharacter(uid);
		}
	}
}
