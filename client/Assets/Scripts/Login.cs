using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Login : MonoBehaviour {

	// Use this for initialization
	void Start () {
		Button btn;
		btn = GetComponent<Button>();
		btn.onClick.AddListener(OnButtonClick);
	}

	// Update is called once per frame
	void Update () {

	}

	void OnButtonClick() {
		Debug.Log("Hello");
	}
}
