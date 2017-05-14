using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using zprotobuf;
using client_zproto;

public class ThirdPersonManager : MonoBehaviour {

	//component
	public GameObject thirdperson;

	//member
	private  static ThirdPersonManager inst = null;
	private Dictionary<int, ThirdPerson> pool = new Dictionary<int, ThirdPerson>();

	void Start() {
		a_sync ack = new a_sync();
		NetProtocol.Instance.Register(ack, ack_sync);
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

	private void ack_sync(int err, wire obj) {
		a_sync ack = (a_sync)obj;
		Debug.Log("SYNC:"+ ack.pos.x);
	}
}
