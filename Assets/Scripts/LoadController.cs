using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadController : MonoBehaviour {

	private static LoadController music;
	//public TimeManager time;
	// Use this for initialization
	void Awake() {
		
		DontDestroyOnLoad (this);

		if (music == null) {
			music = this;
			//time.music = this.GetComponent<AudioSource> ();
				
		} else {
			Destroy(gameObject);
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
