using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Console : MonoBehaviour {
	public Text log;
	private int logcount = 0;
	public void Log(string name) {
		logcount = logcount + 1;
		if (logcount > 10) {
			log.text = name;
			logcount = 0;
		} else {
			log.text += name + "\r\n";
		}
	}

	void Awake() {
		GameInstance.console = this;
	}
}
