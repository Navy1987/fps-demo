using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using zprotobuf;
using client_zproto;

public class LoginWnd : MonoBehaviour {
	public int test = 3;
	public Button login_btn;
	// Use this for initialization
	void Start () {
		login_btn.onClick.AddListener(login);
		a_foo ack = new a_foo();
		NetProtocol.Instance.Register(ack, logined);
	}

	int logined(int err, wire obj) {
		a_foo foo = (a_foo)obj;
		Debug.Log("LoginEd:" + test + ":" + foo.hello + ":" + foo.world);
		return 0;
	}

	// Update is called once per frame
	void Update () {

	}

	void login() {
		r_foo f = new r_foo();
		f.hello = 3;
		f.world = "hello";
		NetProtocol.Instance.Send(f);
		Debug.Log("click!" + test);
		//SceneManager.Instance.SwitchScene("GameScene");
	}
}
