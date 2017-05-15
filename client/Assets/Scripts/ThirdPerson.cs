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
	}

	void Update () {

	}

	public void Sync(Vector3 pos, Quaternion rot) {
		if (animator == null)
			return ;
		float forward_amount = Vector3.Distance(pos, transform.position);
		animator.SetFloat("Forward", forward_amount * speed_multiplier,
			0.1f, Time.deltaTime);

		Vector3 move = pos - transform.position;
		move.Normalize();
		move = transform.InverseTransformDirection(move);
		/*
		TODO:fix the rotation
		//move = Vector3.ProjectOnPlane(move, m_GroundNormal);
		float angle = Mathf.Atan2(move.x, move.z);
		if (uid == 4)
			Debug.Log("+++SyncPos:" + uid + ":" + pos + transform.position + ":" + forward_amount + ":" + angle);
		animator.SetFloat("Turn", -angle, 0.1f, Time.deltaTime);
		*/
		transform.rotation = rot;
		animator.speed = 1.0f;
	}

	public void OnAnimatorMove()
	{
		if (Time.deltaTime > 0) {
			Vector3 v = animator.deltaPosition /
				Time.deltaTime;
			v.y = RB.velocity.y;
			RB.velocity = v;
		}
		if (uid == 4)
		Debug.Log("DeltaPosition:"+ uid + ":" + animator.deltaPosition + transform.position);
	}


}
