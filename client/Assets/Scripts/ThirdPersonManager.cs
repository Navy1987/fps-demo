using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonManager : MonoBehaviour {

	//component
	public GameObject thirdperson;

	//member
	private  static ThirdPersonManager inst = null;
	private Dictionary<int, ThirdPerson> pool = null;

	void Start() {
		pool = new Dictionary<int, ThirdPerson>();
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
		Debug.Assert(!pool.ContainsKey(uid));
		GameObject obj = Instantiate(thirdperson,
			new Vector3(0, 0, 0), Quaternion.identity);
		ThirdPerson p = obj.GetComponent<ThirdPerson>();
		Debug.Log("CreateCharacter:"+p);
		p.Uid = uid;
		pool[uid] = p;
		return p;
	}
}
