using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class main : MonoBehaviour {
	public GameObject loginwnd;
	// Use this for initialization
	void Start () {
		Instantiate(loginwnd, new Vector3(0, 0, 0), Quaternion.identity);
	}

	// Update is called once per frame
	void Update () {

	}
}
