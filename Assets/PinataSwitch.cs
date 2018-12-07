using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PinataSwitch : MonoBehaviour {

	public GameObject platform;
	public GameObject anchor;
	public LineRenderer pinataString;
	public PinataPhysics pinata; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

		
	}

	void OnTriggerEnter2D(Collider2D coll) {

		if (coll.gameObject.tag == "bullet" || coll.gameObject.tag == "Player") {
			//Debug.Log("?");
			DropPinata();

		} 

	}

	void DropPinata() {

		//Debug.Log("???");
		//Destroy(platform);
		//Destroy(anchor);
		if (pinataString) {
			pinataString.enabled = false;
		}
		pinata.cut = true;
		Destroy(this.gameObject);

	}
}
