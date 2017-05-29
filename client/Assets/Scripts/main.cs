using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class main : MonoBehaviour {
	public string login_ip = "127.0.0.1";
	public string login_port = "8001";
	public string gate_ip = "127.0.0.1";
	public string gate_port = "8001";

	void onConnect() {
		Debug.Log("[Main] OnConnected");
	}

	void onLoginClose() {
		Debug.Log("[Main] OnLoginClose");
	}

	void onGateClose() {
		Debug.Log("[Main] OnGateClose");
		NetInstance.Login.Reconnect();
		SceneManager.Instance.SwitchScene("LoginScene");
	}

	void Start () {
		NetInstance.Login.Connect(login_ip, Int32.Parse(login_port), onConnect, onLoginClose);
		NetInstance.Gate.Connect(gate_ip, Int32.Parse(gate_port), onConnect, onGateClose);
		SceneManager.Instance.SwitchScene("LoginScene");
	}

	// Update is called once per frame
	void Update () {
		NetInstance.Update();
	}
}
