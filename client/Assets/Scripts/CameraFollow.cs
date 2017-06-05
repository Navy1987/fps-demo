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

	public void LateUpdate() {
		if (follow_target == null)
			return ;
		var pos = follow_target.transform.position;
		var rot = follow_target.transform.localRotation;

		var src_rot = follow_camera.transform.localRotation;
		var dst_rot = rot * Quaternion.Euler(follow_target.Shadow.rot.eulerAngles.x, 0.0f, 0.0f);
		follow_camera.transform.localRotation = Quaternion.Slerp(src_rot, dst_rot, 0.5f);
		follow_camera.transform.position = pos;
		follow_camera.transform.position += follow_camera.transform.rotation * camera_pos;
	}
}
