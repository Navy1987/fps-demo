using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponShutGun01 : WeaponBase {
	private LineRenderer aimLine;
	private GameObject firepoint;
	private GameObject righthand;

	private IEnumerator ShotEffectCo()
	{
		aimLine.enabled = true;
		yield return new WaitForSeconds(0.7f);
		aimLine.enabled = false;
	}

	public override void Equip(GameObject lefthand, GameObject righthand) {
		aimLine= GetComponent<LineRenderer>();
		this.righthand = righthand;
		firepoint = Tool.FindChild(transform, "FirePoint");
		transform.parent = this.righthand.transform;
		Debug.Log("Equip:" + this.righthand);
	}

	public override void Unload() {

	}

	public override void Shoot(Vector3 dst) {
		Vector3 src = firepoint.transform.position;
		StartCoroutine (ShotEffectCo());
		aimLine.SetPosition (0, src);
		aimLine.SetPosition (1, dst);
		Debug.Log("ShootEffect Shoot:" + src + dst);
	}
}

