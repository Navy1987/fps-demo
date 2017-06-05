using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponBase : MonoBehaviour {
	private LineRenderer aimLine;
	private GameObject firepoint;
	private GameObject righthand;

	private IEnumerator ShotEffectCo()
	{
		aimLine.enabled = true;
		yield return new WaitForSeconds(0.7f);
		aimLine.enabled = false;
	}

	public void Equip(GameObject lefthand, GameObject righthand) {
		aimLine= GetComponent<LineRenderer>();
		this.righthand = righthand;
		firepoint = gameObject;
		transform.parent = righthand.transform;
		Debug.Log("Equip:" + righthand);
	}

	public void Unload() {

	}

	public void Shoot(Vector3 dst) {
		Vector3 src = firepoint.transform.position;
		StartCoroutine (ShotEffectCo());
		aimLine.SetPosition (0, src);
		aimLine.SetPosition (1, dst);
		Debug.Log("ShootEffect Shoot:" + src + dst);

	}

	void FixedUpdate() {
		if (righthand == null)
			return ;
		//weapon.transform.position = righthand.transform.position;
	}
}

