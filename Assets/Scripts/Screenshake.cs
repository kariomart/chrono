using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Screenshake : MonoBehaviour {

	public Vector3 defaultCameraPos;
	Vector3 weightedDirection;
	float screenshakeTimer = 0;
	float thisMagnitude = 0;

	// Use this for initialization
	void Start () {
		defaultCameraPos = transform.position;
		
	}
			
	// Update is called once per frame
	void Update () {
		if (screenshakeTimer > 0) {
			Vector3 shakeDirection = ((Vector3)Random.insideUnitCircle + weightedDirection).normalized * thisMagnitude * Mathf.Clamp01(screenshakeTimer);

			Vector3 result = defaultCameraPos + shakeDirection;
			result.z = -10;
			transform.position = result;
			screenshakeTimer -= Time.deltaTime;


		}
	}

	public void SetScreenshake(float magnitude, float duration, Vector3 direction) {
		thisMagnitude = magnitude;
		screenshakeTimer = duration;
		weightedDirection = weightedDirection;

	}

	public void SetScreenshake(float magnitude, float duration) {
		SetScreenshake (magnitude, duration, Vector3.zero);

	}


}
