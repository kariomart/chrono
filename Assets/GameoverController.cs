using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameoverController : MonoBehaviour {

	TextMeshProUGUI text;


	void Awake() {
		


	}

	// Use this for initialization
	void Start () {

		text = GetComponent <TextMeshProUGUI> ();
//		Debug.Log (text);
		text.SetText ("the winner is " + GameMaster.me.winner + "!");
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
