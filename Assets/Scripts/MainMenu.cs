using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Kino;
using UnityEngine.Audio;


public class MainMenu : MonoBehaviour {
	
	public Player p1;
	public Player p2;
	public GameObject TV;
	public AnalogGlitchController analogGlitchController;
	AnalogGlitch glitch;
	public int timer;
	public int maxTime;

	public AudioMixer ambienceMix;
	public AudioSource ambience;

	bool gameStarted;


	// Use this for initialization
	void Start () {
		p1 = ReInput.players.GetPlayer(0);
		p2 = ReInput.players.GetPlayer(1);
		analogGlitchController = Camera.main.GetComponent<AnalogGlitchController>();
		glitch = Camera.main.GetComponent<AnalogGlitch>();
		ambienceMix = ambience.outputAudioMixerGroup.audioMixer;
	}
	
	// Update is called once per frame
	void Update () {
		
		if ((p1.GetButtonDown("Start") || p2.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Space))&& !gameStarted){
			gameStarted = true;
			analogGlitchController.enabled = false;
			StartCoroutine(startGame());
//			TV.SetActive(true);
		}

		if (gameStarted && timer <= maxTime) {
			glitch.colorDrift += 0.01f;
			glitch.scanLineJitter += 0.01f;
			float d;
			ambienceMix.GetFloat("Distortion", out d);
			ambienceMix.SetFloat("Distortion", d + 0.0035f);
			timer ++;
		}
		
		else if (gameStarted && timer > maxTime) {
			//SceneManager.LoadSceneAsync(Random.Range(1, 5));
			// StartCoroutine(startGame());

		}
	}

	IEnumerator startGame() {

		yield return new WaitForSeconds(2);
		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Random.Range(1, 5));

        // Wait until the asynchronous scene fully loads
        while (!asyncLoad.isDone)
        {
            yield return null;
        }
    }

	
}
