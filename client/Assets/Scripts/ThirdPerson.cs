using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPerson : MonoBehaviour {
	//configuration
	public float moving_turnspeed = 360f;
	public float turn_speed = 180f;
	public float speed_multiplier = 10f;
	//component
	private Rigidbody RB;
	private Animator animator;

	//turn amount
	private float turn_amount;
	private Vector3 sync_src_pos;
	private Vector3 sync_dst_pos;
	private Quaternion sync_src_rot;
	private Quaternion sync_dst_rot;
	private float sync_time;
	private float forward_amount;
	//camera
	private Camera playercamera = null;
	private Vector3 camera_pos;
	private Quaternion camera_rot;

	private int uid;
	public int Uid {
		get { return uid; }
		set { uid = value; }
	}

	void Start () {
		RB = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		RB.constraints =
			RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezeRotationZ;
		sync_src_pos = transform.position;
		sync_src_rot = transform.rotation;
	}

	public void LookAt(Camera c) {
		Debug.Log("LookAt");
		playercamera = c;
		camera_pos = playercamera.transform.position;
		camera_rot = playercamera.transform.rotation;
	}

	void Update () {

	}

	void FixAnimator() {
		//position
		float forward_amount = Vector3.Distance(sync_dst_pos, transform.position);
		if (forward_amount > 0.1f)
			forward_amount = 1.0f;
		else
			forward_amount = 0.0f;
		animator.SetFloat("Forward", forward_amount, 0.1f, Time.deltaTime);
		//rotation
		/*
		Vector3 face_src = transform.rotation * Vector3.forward;
		Vector3 new_src = sync_dst_rot * Vector3.forward;
		Vector3 direct = Vector3.Cross(face_src, new_src).normalized;
		float sign = direct == Vector3.up ? 1 : -1;
		Debug.Log("FixUpdate:" + uid + ":" + turn_amount + transform.rotation + sync_dst_rot);
		animator.SetFloat("Turn", turn_amount, 0.1f, Time.deltaTime);
		*/
	}

	void FixedUpdate() {
		FixAnimator();
	}

	public void Sync(Vector3 pos, Quaternion rot) {
		if (animator == null)
			return ;
		sync_dst_pos= pos;
		sync_src_pos = transform.position;
		sync_src_rot = transform.rotation;
		sync_dst_rot = rot;
		sync_time = Time.time;
		sync_dst_pos.y = 0;
		sync_src_pos.y = 0;
		animator.speed = 1.0f;
		FixAnimator();
		float turn_amount =  (sync_dst_rot.eulerAngles.y - transform.rotation.eulerAngles.y) * Mathf.Deg2Rad;
		animator.SetFloat("Turn", turn_amount * 3, 0.1f, Time.deltaTime);
	}

	public void OnAnimatorMove()
	{
		if (Time.deltaTime > 0) {
			Vector3 v = (sync_dst_pos - transform.position) / 0.10f;
			v.y = RB.velocity.y;
			RB.velocity = v;
			transform.rotation = Quaternion.Lerp(sync_src_rot, sync_dst_rot, (Time.time - sync_time) / 0.1f);
			if (playercamera) {
				var pos = transform.position;
				var rot = transform.rotation;
				playercamera.transform.position = pos;
				playercamera.transform.rotation = rot * camera_rot;
				playercamera.transform.position += playercamera.transform.rotation * camera_pos;
			}
		}
	}


}
