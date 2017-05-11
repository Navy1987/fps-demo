using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPerson : MonoBehaviour {
	//configuration
	public float moving_turnspeed = 360f;
	public float turn_speed = 180f;
	public float speed_multiplier = 1f;
	//component
	private Rigidbody RB;
	private Animator animator;

	//turn amount
	private float turn_amount;
	private float forward_amount;

	private int uid;
	public int Uid {
		get { return uid; }
		set { uid = value; }
	}

	// Use this for initialization
	void Start () {
		RB = GetComponent<Rigidbody>();
		animator = GetComponent<Animator>();
		RB.constraints =
			RigidbodyConstraints.FreezeRotationX |
			RigidbodyConstraints.FreezeRotationY |
			RigidbodyConstraints.FreezeRotationZ;
	}

	// Update is called once per frame
	void Update () {

	}

	void UpdateAnimator(Vector3 move) {
		animator.SetFloat("Forward", forward_amount, 0.1f, Time.deltaTime);
		animator.SetFloat("Turn", turn_amount, 0.1f, Time.deltaTime);
		animator.speed = speed_multiplier;
	}

	public void Move(Vector3 move, bool crouch, bool jump) {
		if (move.magnitude > 1f)
			move.Normalize();
		move = transform.InverseTransformDirection(move);
		turn_amount = Mathf.Atan2(move.x, move.z);
		forward_amount = move.z;
		float turnangle = Mathf.Lerp(turn_speed, moving_turnspeed,
				forward_amount);
		turnangle = turn_amount * turnangle * Time.deltaTime;
		transform.Rotate(0, turnangle, 0);
		UpdateAnimator(move);
	}

	public void OnAnimatorMove() {
		Vector3 v = (animator.deltaPosition * speed_multiplier) / Time.deltaTime;
		v.y = RB.velocity.y;
		RB.velocity = v;
	}
}
