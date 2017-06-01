using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirdPerson : MonoBehaviour {
	//configuration
	public float moving_turnspeed = 360f;
	public float turn_speed = 180f;
	public float speed_multiplier = 10f;
	public float player_ui_scale = 1.0f;
	public Slider player_hp;
	public Vector3 player_hp_offset;
	private AudioSource gunAudio;
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
	private bool camera_follow = false;
	private Camera playercamera = null;
	private Vector3 camera_pos;
	private Quaternion camera_rot;

	private int uid;
	public int Uid {
		get { return uid; }
		set { uid = value; }
	}

	void Start () {
		Debug.Log("ThirdPerson Start");
		RB = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		RB.constraints =
			RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezeRotationZ;
		sync_src_pos = transform.position;
		sync_src_rot = transform.localRotation;
		sync_dst_rot = transform.localRotation;
		playercamera = CameraManager.main;
		gunAudio = GetComponent<AudioSource>();
	}

	public void CameraFollow(bool follow) {
		camera_follow = follow;
		if (follow) {
			playercamera = CameraManager.main;
			Debug.Log("CameraFollow:"+ CameraManager.main);
			camera_pos = playercamera.transform.position;
			camera_rot = playercamera.transform.localRotation;
		}
	}

	void Update () {

	}

	void FixedAnimator() {
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

	void FixedUI() {
		Vector3 pos = transform.position + player_hp_offset;
		float scale = player_ui_scale / Vector3.Distance(transform.position, playercamera.transform.position);
		pos = playercamera.WorldToScreenPoint(pos);
		if (pos.x > 0 && pos.y > 0 && pos.z > 0) {
			player_hp.gameObject.SetActive(true);
			player_hp.transform.position = pos;
			player_hp.transform.localScale = Vector3.one * scale;
		} else {
			player_hp.gameObject.SetActive(false);
		}
	}

	void FixedUpdate() {
		FixedAnimator();
		FixedUI();
	}

	public void Sync(Vector3 pos, Quaternion rot) {
		if (animator == null)
			return ;
		sync_dst_pos= pos;
		sync_src_pos = transform.position;
		sync_src_rot = transform.localRotation;
		sync_dst_rot = rot;

		sync_time = Time.time;
		sync_dst_pos.y = 0;
		sync_src_pos.y = 0;
		animator.speed = 1.0f;
		FixedAnimator();
		float turn_amount =  (sync_dst_rot.eulerAngles.y - transform.localRotation.eulerAngles.y) * Mathf.Deg2Rad;
		animator.SetFloat("Turn", turn_amount * 3, 0.1f, Time.deltaTime);
	}

	public void OnAnimatorMove()
	{
		if (Time.deltaTime > 0.0f) {
			Vector3 v = (sync_dst_pos - transform.position) / 0.10f;
			v.y = RB.velocity.y;
			RB.velocity = v;
			Quaternion dst = Quaternion.Euler(0.0f, sync_dst_rot.eulerAngles.y, 0.0f);
			transform.localRotation = Quaternion.Slerp(sync_src_rot, dst, (Time.time - sync_time) / 0.1f);
			if (camera_follow && playercamera != null) {
				var pos = transform.position;
				var rot = transform.localRotation;
				var updown = Quaternion.Euler(sync_dst_rot.eulerAngles.x, 0.0f, 0.0f);
				camera_rot = Quaternion.Slerp(camera_rot, camera_rot * updown, (Time.time - sync_time) / 0.1f);
				camera_rot = ClampRotationAroundXAxis(camera_rot * updown);
				playercamera.transform.position = pos;
				playercamera.transform.localRotation = rot * camera_rot;
				playercamera.transform.position += playercamera.transform.rotation * camera_pos;
			}
		}
	}

	Quaternion ClampRotationAroundXAxis(Quaternion q)
	{
		q.x /= q.w;
		q.y /= q.w;
		q.z /= q.w;
		q.w = 1.0f;
		float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan (q.x);
		angleX = Mathf.Clamp (angleX, -16.0f, 16.0f);
		q.x = Mathf.Tan (0.5f * Mathf.Deg2Rad * angleX);
		return q;
        }



}
