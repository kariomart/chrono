using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CamControl : MonoBehaviour {

	Vector3 defaultPos;
	float defaultZoom;
	public float maxZoom;
	Camera cam;
	TimeManager time;
	public float speed;
	public float zoomSpeed;

	// Use this for initialization
	void Start () {

		time = GameObject.Find("TimeManager").GetComponent<TimeManager>();
		cam = GetComponent<Camera>();
		defaultPos = transform.position;
		defaultZoom = cam.orthographicSize;
		
	}
	
	// Update is called once per frame
	void FixedUpdate () {

		float step = speed * Time.deltaTime;
		float zStep = zoomSpeed * Time.deltaTime;

		if (time.gameOverSlow) {
//			Debug.Log("goin");
			transform.position = new Vector3(Vector2.MoveTowards(transform.position, time.pos, step).x, Vector2.MoveTowards(transform.position, time.pos, step).y, defaultPos.z);
			cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, maxZoom, zStep);

		} else {
			transform.position = new Vector3(Vector2.MoveTowards(transform.position, defaultPos, step).x, Vector2.MoveTowards(transform.position, defaultPos, step).y, defaultPos.z);
			cam.orthographicSize = Mathf.MoveTowards(cam.orthographicSize, defaultZoom, zStep);
		}


	}
}
