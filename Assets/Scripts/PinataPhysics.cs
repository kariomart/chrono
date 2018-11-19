using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinataPhysics : MonoBehaviour {

	public Rigidbody2D rb;
	
	public Vector2 anchorPoint;
	public float stringLength;
	public float stringStrength;
	public float dis;
	public float gravity;
	public float terminalVel;
	public float cutX;

	public Vector2 vel;

	public LineRenderer line;
	public bool cut;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody2D>();
		line = GetComponent<LineRenderer>();
		anchorPoint = GameObject.Find("BUTTON").transform.position;
		
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
		

		if (transform.position.y <= -5 && cut) {
//			Debug.Log("hmm");
			transform.position = new Vector3(transform.position.x, 7, transform.position.z);
		}
		

		if (!cut) {
			if (dis > stringLength) {
				pos = anchorPoint + ((pos - anchorPoint).normalized * stringLength);
				vel += (anchorPoint - pos) * stringStrength;
			} else {
			}

			vel *= .95f;
		}

		if (cut) {
			float yVel = Mathf.Clamp(vel.y, -terminalVel, terminalVel);
			vel = new Vector2(0, yVel);
		}
		rb.MovePosition((Vector2)transform.position + (vel * Time.fixedDeltaTime));

	}

	void OnCollisionEnter2D(Collision2D coll) {

		if (coll.gameObject.tag == "Player") {

			if (cut) {

			}
		}

	}
}
