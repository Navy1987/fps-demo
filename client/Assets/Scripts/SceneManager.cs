using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneManager : MonoBehaviour {

	private static SceneManager inst = null;
	private GameObject currentScene = null;
	private Dictionary<string, GameObject> pool = new Dictionary<string, GameObject>();

	public static SceneManager Instance {
		get {
			if (inst == null) {
				GameObject go = Resources.Load("Prefabs/SceneManager", typeof(GameObject)) as GameObject;
				go = Instantiate(go) as GameObject;
				inst = go.GetComponent<SceneManager>();
			}
			return inst;
		}
	}

	// Use this for initialization
	void Start () {

	}

	// Update is called once per frame
	void Update () {

	}

	public void SwitchScene(string name) {
		GameObject scene = null;
		if (currentScene)
			currentScene.SetActive(false);
		if (pool.ContainsKey(name))
			scene = pool[name];
		if (scene == null) {
			scene = Resources.Load("Prefabs/" + name, typeof(GameObject)) as GameObject;
			scene = Instantiate(scene) as GameObject;
			Debug.Assert(scene != null);
			pool[name] = scene;
		} else {
			scene.SetActive(true);
		}
		currentScene = scene;
		return ;
	}

}
