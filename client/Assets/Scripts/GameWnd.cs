using System.Collections;
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
		a_join ack = new a_join();
		NetProtocol.Instance.Register(ack, ack_join);
	}

	// Update is called once per frame
	void Update () {

	}

	void on_enter() {
		Debug.Log("Enter");
		r_join req = new r_join();
		req.join = 1;
		NetProtocol.Instance.Send(req);
	}

	void on_leave() {
		Debug.Log("Leave");
		r_join req = new r_join();
		req.join = 0;
		NetProtocol.Instance.Send(req);
	}

	///protocol
	void ack_join(int err, wire obj) {
		a_join ack = (a_join) obj;
		Debug.Log("Ack_Join:" +  err + ":" + ack.uid + ":" + ack.join);
		if (ack.join == 1)
			ThirdPersonManager.Instance.CreateCharacter(ack.uid);
		else
			ThirdPersonManager.Instance.DeleteCharacter(ack.uid);
	}
}
