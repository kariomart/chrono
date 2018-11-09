using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
public class MainMenu : MonoBehaviour {
	public Player p1;
	public Player p2;
	// Use this for initialization
	void Start () {
		p1 = ReInput.players.GetPlayer(0);
		p2 = ReInput.players.GetPlayer(1);
	}
	
	// Update is called once per frame
	void Update () {
		if (p1.GetButtonDown("Start") || p2.GetButtonDown("Start")){
			SceneManager.LoadScene(Random.Range(1, 5));
		}
	}
}
