using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinataPhysics : MonoBehaviour {

	Rigidbody2D rb;
	
	public Vector2 anchorPoint;
	public float stringLength;
	public float stringStrength;
	public float dis;
	public float gravity;

	public Vector2 vel;

	public LineRenderer line;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody2D>();
		line = GetComponent<LineRenderer>();
		anchorPoint = GameObject.Find("anchor").transform.position;
		
	}
	
	// Update is called once per frame
	void Update () {

		line.SetPosition(0, transform.position);
		line.SetPosition(1, anchorPoint);
		
	}

	void FixedUpdate() {

		Vector2 pos = transform.position;
		dis = Vector2.Distance(anchorPoint, pos);
		vel.y -= gravity * Time.fixedDeltaTime;
		

		if (dis > stringLength) {
			pos = anchorPoint + ((pos - anchorPoint).normalized * stringLength);
			vel += (anchorPoint - pos) * stringStrength;
		} else {
		}

	rb.MovePosition(pos + (vel * Time.fixedDeltaTime));



	}
}
