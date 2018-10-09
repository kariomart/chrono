using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;


public class TimeManager : MonoBehaviour {

	public PlayerMovementController player1;
	public PlayerMovementController player2;
	public AudioSource music;
	public PostProcessingProfile FX;
	public PostProcessingProfile DoubleFX;

	public bool gameOverSlow;
	public int slowCounter;
	public Vector2 pos;

	public float globalTimescale;
	public int timeValue;

	public AudioClip slowSound;




	void Awake() {

		// Debug.Log(GameMaster.me);
		// GameMaster.me.findUI ();
		// GameMaster.me.updateUI ();


	}
	// Use this for initialization
	void Start () {


//		Debug.Log(GameMaster.me);
		GameMaster.me.findUI ();
		GameMaster.me.updateUI ();
		GameMaster.me.findSpawnPoints();
		player1 = GameMaster.me.player1;
		player2 = GameMaster.me.player2;

	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (music == null) {

			music = GameObject.FindGameObjectWithTag ("AudioController").GetComponent<AudioSource>();
		}

		globalTimescale = Time.timeScale;

		NormalTime ();

		if (player1 && player2) {

			if (player1.slow || player2.slow) {
				SlowTime ();
			}

			if (player1.speed || player2.speed) {
				SpeedTime ();
			}

			if ((player1.slow && player2.slow) || gameOverSlow) {
				//SlowTime ();
				DoubleSlow();
			} 

			if ((player1.slow && player2.speed) || (player1.speed && player2.slow)) {
				NormalTime ();
			} 
			if (player1.speed && player2.speed) {
				SpeedTime ();
			}

			if (gameOverSlow) {
				slowCounter ++;
			}

			if (slowCounter > 300) {
				gameOverSlow = false;
				slowCounter = 0;
			}

			if (gameOverSlow == false) {
				slowCounter = 0;
			}
		}
			

		
	}

	void SlowTime() {

		Time.timeScale = 0.25f;
		Time.fixedDeltaTime = Time.timeScale * 1/60f; 
		music.pitch = 0.75f;
		PostProcessingBehaviour p = Camera.main.GetComponent<PostProcessingBehaviour> ();
		p.profile = FX;
		p.enabled = true;


	}

	void DoubleSlow() {

		Time.timeScale = 0.08f;
		Time.fixedDeltaTime = Time.timeScale * 1/60f; 
		music.pitch = 0.50f;
		PostProcessingBehaviour p = Camera.main.GetComponent<PostProcessingBehaviour> ();
		p.profile = DoubleFX;
		p.enabled = true;
	}

	void SpeedTime() {

//		Time.timeScale = 2f;
//		Time.fixedDeltaTime = Time.timeScale * 1/60f; 
//		music.pitch = 1.25f;

	}

	void NormalTime() {

		Time.timeScale = 1f;
		Time.fixedDeltaTime = Time.timeScale * 1/60f; 
		music.pitch = 1f;
		Camera.main.GetComponent<PostProcessingBehaviour> ().enabled = false;

	}



}
