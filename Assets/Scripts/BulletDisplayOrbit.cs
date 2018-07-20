using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletDisplayOrbit : MonoBehaviour {

	public float orbitSpeed;
	public float orbitRadii;
	public Vector2 orbOffset;
	public int index;
	public bool on;
	public PlayerMovementController player;

	// Use this for initialization
	void Start () {


	}
	
	// Update is called once per frame
	void Update () {

		//Orbit();
		
	}

	void Orbit () {

		Vector2 orbitPos = (Vector2)transform.position;
		transform.position = orbitPos + (MagicSpells.ToVect(MagicSpells.ToAng(orbitPos, transform.position) + orbitSpeed) * orbitRadii);

	}
}
