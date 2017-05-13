using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour {
	void Start () {
		NetProtocol.Instance.Connect("127.0.0.1", 9009);
		SceneManager.Instance.SwitchScene("LoginScene");
	}

	// Update is called once per frame
	void Update () {
		NetProtocol.Instance.Update();
	}
}
