using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbitController : MonoBehaviour {

	PlayerMovementController player;
	int index;
	float angle;
	public float orbitDistance;
	public float orbitSpeed;

	// Use this for initialization
	void Start () {

		player = GetComponentInParent<PlayerMovementController>();
		angle = 360 / player.amountOfBullets;
		updateOrbPositions();
		
	}
	
	// Update is called once per frame
	void Update () {

		transform.eulerAngles = new Vector3(transform.eulerAngles.x, transform.eulerAngles.y, transform.eulerAngles.z + orbitSpeed);
		
	}

	void updateOrbPositions() {

		foreach(Transform orbParent in transform) {
			Transform orb = orbParent.transform.GetChild(0).transform;
			Debug.Log(orb.name);
			orb.localPosition = new Vector3(orbitDistance, 0, 0);
			orbParent.eulerAngles = new Vector3(orbParent.eulerAngles.x, orbParent.eulerAngles.y, angle * index);
			index ++;
		}

	}
}
