using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MapPart{
	public Bounds bound;
	public GameObject prefab;
};

class MapData {
	public MapPart info;
	public GameObject inst;
	public MapData(MapPart map) {
		GameObject obj;
		info = map;
		obj = GameObject.Instantiate(info.prefab,
			info.bound.center, Quaternion.identity);
		inst = obj;
	}
	public MapData() {
		info = null;
		inst = null;
	}
};

public class WorldMap : MonoBehaviour {
	public MapPart[] maplist;
	private Vector3 EXPAND = Vector3.one * 20.0f;
	private Bounds mapbound = new Bounds(Vector3.zero, Vector3.zero);
	private Dictionary<MapPart, MapData>[] map_pingpong = new Dictionary<MapPart, MapData>[2];
	private Dictionary<MapPart, MapData> map = null;

	private void Awake() {
		Debug.Log("Awake");
		map_pingpong[0] = new Dictionary<MapPart, MapData>();
		map_pingpong[1] = new Dictionary<MapPart, MapData>();
		map = map_pingpong[0];
	}

	private Dictionary<MapPart, MapData> toggle() {
		if (map == map_pingpong[0])
			return map_pingpong[1];
		else
			return map_pingpong[0];
	}

	public void UpdatePosition(Vector3 pos) {
		if (mapbound.Contains(pos))
			return ;
		mapbound.center = pos;
		mapbound.size = EXPAND;
		var lastmap = map;
		var nextmap = toggle();
		for (int i = 0; i < maplist.Length; i++) {
			var part = maplist[i];
			if (!part.bound.Intersects(mapbound))
				continue;
			Debug.Log("BuildMap" + EXPAND + mapbound + part.bound);
			if (lastmap.ContainsKey(part)) {
				nextmap[part] = lastmap[part];
				lastmap[part] = null;
			} else {
				nextmap[part] = new MapData(part);
			}
		}
		foreach (var item in lastmap) {
			if (item.Value == null)
				continue;
			Destroy(item.Value.inst);
		}
		lastmap.Clear();
		map = nextmap;
	}

}
