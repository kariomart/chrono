using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowZone : MonoBehaviour {


	public bool slow = true;
	public int changeChance;
	SpriteRenderer sprite;


	// Use this for initialization
	void Start () {

		sprite = GetComponent<SpriteRenderer>();
		
	}
	
	// Update is called once per frame
	void Update () {

		int rand = Random.Range(0, changeChance);

		if (rand == 1) {
			//ChangeSpeed();
		}
		
	}


	public void OnCollisionEnter2D(Collision2D coll) {



	}

	void ChangeSpeed() {

		if (slow) {
			slow = false;
			sprite.color = Color.red;
		} else {
			slow = true;
			sprite.color = Color.grey;
		}

	}
}
