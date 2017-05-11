using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginWnd : MonoBehaviour {
	public Button login_btn;
	// Use this for initialization
	void Start () {
		login_btn.onClick.AddListener(login);
	}

	// Update is called once per frame
	void Update () {

	}

	void login() {
		Debug.Log("click!");
		SceneManager.Instance.SwitchScene("GameScene");
	}
}
