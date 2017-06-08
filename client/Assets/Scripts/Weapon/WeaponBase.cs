using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class WeaponBase : MonoBehaviour {
	public abstract void Equip(GameObject lefthand, GameObject righthand);
	public abstract void Unload();
	public abstract void Shoot(Vector3 dst);
}

