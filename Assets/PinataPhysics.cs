using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinataPhysics : MonoBehaviour {

	Rigidbody2D rb;
	
	public Vector2 anchorPoint;
	public float maxDis;
	public float dis;
	public float gravity;

	public Vector2 vel;


	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody2D>();
		//anchorPoint = transform.parent.position;
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate() {

		Vector2 pos = transform.position;
		dis = Vector2.Distance(anchorPoint, pos);
		vel.y -= gravity * Time.fixedDeltaTime;

		if (dis > maxDis) {
			pos = anchorPoint + ((pos - anchorPoint).normalized * maxDis);
			vel += (pos - anchorPoint) * 4f;
		}

	rb.MovePosition(pos + (vel * Time.fixedDeltaTime));



	}
}
