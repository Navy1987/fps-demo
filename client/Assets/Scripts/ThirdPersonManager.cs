using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zprotobuf;
using client_zproto;

public class ThirdPersonManager : MonoBehaviour {
	//component
	public GameObject thirdperson;

	//member
	private const int RESOLUTION = 10000;
	private  static ThirdPersonManager inst = null;
	private Dictionary<int, ThirdPerson> pool = new Dictionary<int, ThirdPerson>();

	//////////unity interface
	void Start() {
		ProtocolStart();
	}


	/////////interface
	public static ThirdPersonManager Instance {
		get {
			if (inst == null)
				inst = GameObject.FindObjectOfType<ThirdPersonManager>();
			return inst;
		}
	}

	public ThirdPerson CreateCharacter(int uid, Vector2 pos) {
		if (pool.ContainsKey(uid))
			return pool[uid];
		GameObject obj = Instantiate(thirdperson,
			new Vector3(pos.x, 0, pos.y), Quaternion.identity);
		ThirdPerson p = obj.GetComponent<ThirdPerson>();
		Debug.Log("CreateCharacter:" + uid + p + p.transform.position);
		p.Uid = uid;
		pool[uid] = p;
		return p;
	}

	public ThirdPerson GetCharacter(int uid) {
		if (!pool.ContainsKey(uid))
			return null;
		return pool[uid];
	}

	public void DeleteCharacter(int uid) {
		if (!pool.ContainsKey(uid))
			return ;
		Debug.Log("DeleteCharacter:" + uid);
		ThirdPerson p = pool[uid];
		pool[uid] = null;
		Destroy(p.gameObject);
	}

	public void Shoot(int a, int b, Vector3 shoot) {
		/*
		r_shoot req = new r_shoot();
		req.a = a;
		req.b = b;
		req.shoot = new vector3();
		req.shoot.x = (int)(shoot.x * RESOLUTION);
		req.shoot.y = (int)(shoot.y * RESOLUTION);
		req.shoot.z = (int)(shoot.z * RESOLUTION);
		Debug.Log("ShootSend x:" + req.shoot.x + ":" + req.shoot.y + ":" + req.shoot.z + shoot);
		NetInstance.Gate.Send(req);
		*/
	}
	////////////net protocol

	void ProtocolStart() {
		a_move @a_move = new a_move();
		NetInstance.Gate.Register(@a_move, ack_move);
	}


	public void SyncCharacter(int uid) {
		ThirdPerson player = GetCharacter(uid);
		r_move @r_move = new r_move();
		@r_move.pos = new vector2();
		@r_move.rot = new rotation();
		Tool.ToProto(ref @r_move.pos, player.transform.position);
		Tool.ToProto(ref @r_move.rot, player.transform.rotation);
		NetInstance.Gate.Send(@r_move);
		Debug.Log("r_move");
	}

	private void ack_move(int err, wire obj) {
		a_move ack = (a_move)obj;
		Debug.Log("a_move:" + err + ":" + ack.uid + ack.pos);
		int uid = ack.uid;
		if (Player.Instance.Uid == uid)
			return ;
		ThirdPerson p = GetCharacter(uid);
		if (p == null) {
			Debug.Log("ASYC UID NULL:" + uid);
			return ;
		}
		Vector3 pos = new Vector3();
		Quaternion rot = new Quaternion();
		Tool.ToNative(ref pos, ack.pos);
		Tool.ToNative(ref rot, ack.rot);
		p.MoveTo(pos, rot, false, false);
		Debug.Log("ack_move uid:" + uid + pos + rot);
	}
}
