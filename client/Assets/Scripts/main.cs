using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class main : MonoBehaviour {
	public string server_ip = "127.0.0.1";
	public string server_port = "9009";

	void Start () {
		SceneManager.Instance.SwitchScene("LoginScene");
	}

	// Update is called once per frame
	void Update () {
		if (!NetProtocol.Instance.isConnected()) {
			NetProtocol.Instance.Connect(server_ip,
				Int32.Parse(server_port));
		}
		NetProtocol.Instance.Update();
	}
}
