using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zprotobuf;
using client_zproto;

public class ThirdPersonManager : MonoBehaviour {

	//component
	public GameObject thirdperson;
	public Camera playercamera;

	//member
	private const int RESOLUTION = 10000;
	private  static ThirdPersonManager inst = null;
	private Dictionary<int, ThirdPerson> pool = new Dictionary<int, ThirdPerson>();

	void Start() {
		a_sync ack = new a_sync();
		NetInstance.Gate.Register(ack, ack_sync);
	}

	void Update () {

	}

	public static ThirdPersonManager Instance {
		get {
			if (inst == null)
				inst = GameObject.FindObjectOfType<ThirdPersonManager>();
			return inst;
		}
	}

	public ThirdPerson CreateCharacter(int uid) {
		Debug.Log("CreateCharacter:" + uid);
		if (pool.ContainsKey(uid))
			return pool[uid];
		GameObject obj = Instantiate(thirdperson,
			new Vector3(0, 0, 0), Quaternion.identity);
		ThirdPerson p = obj.GetComponent<ThirdPerson>();
		Debug.Log("CreateCharacter:"+p);
		p.Uid = uid;
		if (uid == Player.Instance.Uid)
			p.LookAt(playercamera);
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

	public void SyncCharacter(int uid) {
		ThirdPerson player = GetCharacter(uid);
		r_sync sync = new r_sync();
		sync.pos = new vector3();
		sync.rot = new rotation();
		sync.pos.x = (int)(player.transform.position.x * RESOLUTION);
		sync.pos.y = (int)(player.transform.position.y * RESOLUTION);
		sync.pos.z = (int)(player.transform.position.z * RESOLUTION);
		sync.rot.x = (int)(player.transform.rotation.x * RESOLUTION);
		sync.rot.y = (int)(player.transform.rotation.y * RESOLUTION);
		sync.rot.z = (int)(player.transform.rotation.z * RESOLUTION);
		sync.rot.w = (int)(player.transform.rotation.w * RESOLUTION);
		/*
		Debug.Log("SendSync" + "uid:"+ uid + ":" +
				transform.position + transform.rotation +
				sync.pos.x + "," +sync.pos.y + "," + sync.pos.z
				);
		*/
		NetInstance.Gate.Send(sync);

	}

	private void ack_sync(int err, wire obj) {
		a_sync ack = (a_sync)obj;
		int uid = ack.uid;
		Vector3 pos = new Vector3();
		pos.x = (float)ack.pos.x / RESOLUTION;
		pos.y = (float)ack.pos.y / RESOLUTION;
		pos.z = (float)ack.pos.z / RESOLUTION;
		Quaternion rot = new Quaternion();
		rot.x = (float)ack.rot.x / RESOLUTION;
		rot.y = (float)ack.rot.y / RESOLUTION;
		rot.z = (float)ack.rot.z / RESOLUTION;
		rot.w = (float)ack.rot.w / RESOLUTION;
		//Debug.Log("RecvSync" + "uid:"+ uid + ":" + ack.pos.x + "," + ack.pos.y + "," + ack.pos.z + pos + rot);
		if (Player.Instance.Uid == uid)
			return ;
		ThirdPerson p = GetCharacter(uid);
		if (p == null) {
			Debug.Log("ASYC UID NULL:" + uid);
			return ;
		}
		p.Sync(pos, rot);
	}
}
