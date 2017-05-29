using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Security.Cryptography;
using zprotobuf;
using client_zproto;

public class LoginWnd : MonoBehaviour {
	public InputField user_name;
	public InputField user_passwd;
	public Button login_btn;
	public Button create_btn;


	private int uid = 0;
	private int session = 0;

	void Start () {
		user_name.text = "findstr";
		user_passwd.text = "asdfg";
		//event
		create_btn.onClick.AddListener(on_create);
		login_btn.onClick.AddListener(on_login);
		//protocol
		a_create create = new a_create();
		a_login login = new a_login();
		a_challenge challenge = new a_challenge();
		a_login_gate login_gate = new a_login_gate();
		NetInstance.Login.Register(create, ack_create);
		NetInstance.Login.Register(challenge, ack_challenge);
		NetInstance.Login.Register(login, ack_login);
		NetInstance.Gate.Register(login_gate, ack_login_gate);
	}

	void Update () {

	}

	byte[] sha1(string passwd) {
		ASCIIEncoding enc = new ASCIIEncoding();
		byte[] hash = enc.GetBytes(passwd);
		SHA1 sha = new SHA1CryptoServiceProvider();
		return sha.ComputeHash(hash);
	}

	byte[] hmac(byte[] passwd, string text) {
		ASCIIEncoding enc = new ASCIIEncoding();
		byte[] hash = enc.GetBytes(text);
		HMACSHA1 hmac = new HMACSHA1(passwd);
		return hmac.ComputeHash(hash);
	}

	void on_create() {
		r_create req = new r_create();
		byte[] str = sha1(user_passwd.text);
		req.user = Encoding.Default.GetBytes(user_name.text);
		req.passwd = str;
		NetInstance.Login.Send(req);
		Debug.Log("OnCreate" + user_name.text + ":" + BitConverter.ToString(str));
		return ;
	}

	void on_login() {
		Debug.Log("OnLogin");
		r_challenge req = new r_challenge();
		NetInstance.Login.Send(req);
		return ;
	}

	void ack_create(int err, wire obj) {
		a_create ack = (a_create)obj;
		Debug.Log("create!" + ack.uid);
		return ;
	}

	void ack_challenge(int err, wire obj) {
		a_challenge ack = (a_challenge) obj;
		string str = user_passwd.text;
		byte[] passwd = sha1(str);
		byte[] hash = hmac(passwd, Encoding.Default.GetString(ack.randomkey));
		r_login req = new r_login();
		Debug.Log("challenge!" + Encoding.Default.GetString(ack.randomkey) +
				":" + BitConverter.ToString(hash));
		req.gateid = 1;
		req.user = Encoding.Default.GetBytes(user_name.text);
		req.passwd = hash;
		NetInstance.Login.Send(req);
		return ;
	}

	void req_login_gate() {
		Debug.Log("Req_Login_Gate");
		r_login_gate req = new r_login_gate();
		req.uid = uid;
		req.session = session;
		NetInstance.Gate.Send(req);
	}

	void ack_login(int err, wire obj) {
		a_login ack = (a_login) obj;
		if (err == 0) {
			uid = ack.uid;
			session = ack.session;
			NetInstance.Login.Close();
			req_login_gate();
			Debug.Log("Login uid:" + uid + ack.session);
		}
		Debug.Log("login! uid:" + uid + " err:" + err);
	}

	void ack_login_gate(int err, wire obj) {
		Debug.Log("login_gate!" + err);
		Player.Instance.Init(uid);
		SceneManager.Instance.SwitchScene("GameScene");
	}
}
