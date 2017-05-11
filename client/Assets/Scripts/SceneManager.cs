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
				GameObject go = new GameObject();
				inst = go.AddComponent<SceneManager>();
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
			scene = Resources.Load("Assets/Prefabs/" + name) as GameObject;
			Debug.Log("SwitchScene:"+ this + ":" + name + ":" + scene);
			Debug.Assert(scene != null);
			pool[name] = scene;
		}
		currentScene = scene;
		return ;
	}

}
