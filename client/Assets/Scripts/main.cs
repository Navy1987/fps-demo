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

	void Start () {
		SceneManager.Instance.SwitchScene("LoginScene");
		NetProtocol.Instance.InitLoginAddr(login_ip, Int32.Parse(login_port));
		NetProtocol.Instance.InitGateAddr(gate_ip, Int32.Parse(gate_port));
		NetProtocol.Instance.Switch(NetProtocol.LOGIN, null, null);
		NetProtocol.Instance.Connect();
	}

	// Update is called once per frame
	void Update () {
		NetProtocol.Instance.Update();
	}
}
