﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Kino;

public class MainMenu : MonoBehaviour {
	
	public Player p1;
	public Player p2;
	public GameObject TV;
	public AnalogGlitchController analogGlitchController;
	AnalogGlitch glitch;
	public int timer;
	public int maxTime;

	bool gameStarted;


	// Use this for initialization
	void Start () {
		p1 = ReInput.players.GetPlayer(0);
		p2 = ReInput.players.GetPlayer(1);
		analogGlitchController = Camera.main.GetComponent<AnalogGlitchController>();
		glitch = Camera.main.GetComponent<AnalogGlitch>();
	}
	
	// Update is called once per frame
	void Update () {
		
		if (p1.GetButtonDown("Start") || p2.GetButtonDown("Start") && !gameStarted){
			gameStarted = true;
			analogGlitchController.enabled = false;
//			TV.SetActive(true);
		}

		if (gameStarted && timer <= maxTime) {
			glitch.colorDrift += 0.01f;
			glitch.scanLineJitter += 0.01f;
			timer ++;
		}
		
		else if (gameStarted && timer > maxTime) {
			//SceneManager.LoadScene(Random.Range(1, 5));
			StartCoroutine(startGame());
		}
	}

	IEnumerator startGame() {
		yield return new WaitForSeconds(1f);
		SceneManager.LoadScene(Random.Range(1, 5));
	}

	
}
