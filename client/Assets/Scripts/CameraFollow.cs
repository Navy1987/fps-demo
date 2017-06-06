using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow {
	private Vector3 camera_pos;
	private Quaternion camera_rot;
	private ThirdPerson follow_target;
	private Camera maincamera;

	public void Start() {
		maincamera = CameraManager.main;
		camera_pos = maincamera.transform.position;
	}

	public void Follow(ThirdPerson player) {
		follow_target = player;
	}

	private void MainFollow() {
		var pos = follow_target.transform.position;
		var rot = follow_target.transform.localRotation;

		var src_rot = maincamera.transform.localRotation;
		var dst_rot = rot * Quaternion.Euler(follow_target.Shadow.rot.eulerAngles.x, 0.0f, 0.0f);
		maincamera.transform.localRotation = Quaternion.Slerp(src_rot, dst_rot, 0.5f);
		maincamera.transform.position = pos;
		maincamera.transform.position += maincamera.transform.rotation * camera_pos;
	}

	public void LateUpdate() {
		if (follow_target == null)
			return ;
		MainFollow();
	}
}
