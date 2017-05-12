using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using client_zproto;

public class main : MonoBehaviour {
	// Use this for initialization
	private NetSocket s = null;
	private bool send = false;
	void Start () {
		s = new NetSocket();
		s.Connect("192.168.2.118", 9009);
		SceneManager.Instance.SwitchScene("LoginScene");
	}

	// Update is called once per frame
	void Update () {
		if (send == false && s.Status == NetSocket.CONNECTED) {
			byte[] dat = null;
			foo f = new foo();
			f.hello = 3;
			f.world = "hello";
			f._serialize(out dat);
			short len = (short)(4 + dat.Length);
			byte[] pack = new byte[2 + len];
			len = System.Net.IPAddress.HostToNetworkOrder(len);
			int cmd = 1;
			System.BitConverter.GetBytes(len).CopyTo(pack, 0);
			System.BitConverter.GetBytes(cmd).CopyTo(pack, 2);
			dat.CopyTo(pack, 6);
			s.Send(pack);
			send = true;
		}
	}
}
