using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow {
	private Vector3 camera_pos;
	private Quaternion camera_rot;
	private ThirdPerson follow_target;
	private Camera follow_camera;

	public void Start() {
		follow_camera = CameraManager.main;
		camera_pos = follow_camera.transform.position;
	}

	public void Follow(ThirdPerson player) {
		follow_target = player;
	}

	private void Follow(Camera follow) {
		var pos = follow_target.transform.position;
		var rot = follow_target.transform.localRotation;

		var src_rot = follow.transform.localRotation;
		var dst_rot = rot * Quaternion.Euler(follow_target.Shadow.rot.eulerAngles.x, 0.0f, 0.0f);
		follow.transform.localRotation = Quaternion.Slerp(src_rot, dst_rot, 0.5f);
		follow.transform.position = pos;
		follow.transform.position += follow.transform.rotation * camera_pos;
	}

	public void LateUpdate() {
		if (follow_target == null)
			return ;
		Follow(follow_camera);
		Follow(CameraManager.ui);
	}
}
