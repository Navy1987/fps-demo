using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using client_zproto;

class Tool {
	private const int RESOLUTION = 100;
	public static void ToProto(ref vector2 dst, Vector2 src) {
		dst.x = (int)(src.x * RESOLUTION);
		dst.z = (int)(src.y * RESOLUTION);
	}

	public static void ToProto(ref vector2 dst, Vector3 src) {
		dst.x = (int)(src.x * RESOLUTION);
		dst.z = (int)(src.z * RESOLUTION);
	}

	public static void ToProto(ref rotation dst, Quaternion src) {
		dst.x = (int)(src.x * RESOLUTION);
		dst.y = (int)(src.y * RESOLUTION);
		dst.z = (int)(src.z * RESOLUTION);
		dst.w = (int)(src.w * RESOLUTION);
	}

	public static void ToNative(ref Vector2 dst, vector2 src) {
		dst.x = (float)src.x / (float)RESOLUTION;
		dst.y = (float)src.z / (float)RESOLUTION;
	}

	public static void ToNative(ref Vector3 dst, vector2 src) {
		dst.x = (float)src.x / (float)RESOLUTION;
		dst.z = (float)src.z / (float)RESOLUTION;
		dst.y = 0.0f;
	}

	public static void ToNative(ref Quaternion dst, rotation src) {
		dst.x = (float)src.x / (float)RESOLUTION;
		dst.y = (float)src.y / (float)RESOLUTION;
		dst.z = (float)src.z / (float)RESOLUTION;
		dst.w = (float)src.w / (float)RESOLUTION;
	}

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


