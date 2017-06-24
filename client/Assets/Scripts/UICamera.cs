using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICamera : MonoBehaviour {
	private Camera _camera;

	void Awake() {
		Start();
	}

	// Use this for initialization
	void Start () {
		_camera = GetComponent<Camera>();
		CameraManager.ui = _camera;
	}

	// Update is called once per frame
	void Update () {

	}
}
