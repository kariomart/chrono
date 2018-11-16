using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelSettings : MonoBehaviour {

	public static LevelSettings me;

	public Transform spawn1;
	public Transform spawn2;

	public int startBullets;


	void Awake() {

		me = this;
		if (GameMaster.me == null){
			Instantiate(Resources.Load("MANAGERS"), Vector3.zero, Quaternion.identity);
			GameMaster.me.managers = GameObject.Find("MANAGERS(Clone)");
		}
		GameMaster.me.initializeLevel();
	}
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
