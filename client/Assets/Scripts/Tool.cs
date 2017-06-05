using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class Tool {
	public static GameObject FindChild(Transform parent, string childName) {
		var f = parent.Find(childName);
		if (f != null)
			return f.gameObject;
		foreach (Transform child in parent) {
			var obj = FindChild(child, childName);
			if (obj != null)
				return obj;
		}
		return null;
	}
	public static Quaternion ClampRotationAroundXAxis(Quaternion q) {
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;
		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);
		angleX = Mathf.Clamp(angleX, -15.0f, 15.0f);
		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);
		return q;
        }
}


