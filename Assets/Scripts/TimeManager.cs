using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PostProcessing;


public class TimeManager : MonoBehaviour {

	public PlayerMovementController player1;
	public PlayerMovementController player2;
	public AudioSource music;
	public AudioMixer musicMix;
	public PostProcessingProfile FX;
	public PostProcessingProfile DoubleFX;

	public bool gameOverSlow;
	public int slowCounter;
	public Vector2 pos;

	public float globalTimescale;
	public int timeValue;

	public AudioClip slowSound;

	int lowpassMin = 500;
	int lowpassMax = 22000;

	public ScanlinesEffect scanlines;




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
		scanlines = Camera.main.GetComponent<ScanlinesEffect>();

		if (!musicMix) {
			musicMix = music.outputAudioMixerGroup.audioMixer;
		}

	}
	
	// Update is called once per frame
	void FixedUpdate () {

		if (music == null) {

			music = GameObject.FindGameObjectWithTag ("AudioController").GetComponent<AudioSource>();
		}

		globalTimescale = Time.timeScale;

	

		if (player1 && player2 && !GameMaster.me.GameIsPaused) {

			if (!player1.slow && !player2.slow) {
				NormalTime ();
			}

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
		//musicMix.SetFloat("lowpassFreq", 920);
		// PostProcessingBehaviour p = Camera.main.GetComponent<PostProcessingBehaviour> ();
		// p.profile = FX;
		// p.enabled = true;
		GameMaster.me.AddSlowFX();
		scanlines.displacementSpeed = 0.038f;

	}

	void DoubleSlow() {

		Time.timeScale = 0.08f;
		Time.fixedDeltaTime = Time.timeScale * 1/60f; 
		music.pitch = 0.50f;
		//musicMix.SetFloat("lowpassFreq", 500);
		// PostProcessingBehaviour p = Camera.main.GetComponent<PostProcessingBehaviour> ();
		// p.profile = DoubleFX;
		// p.enabled = true;
		GameMaster.me.AddSlowFX();
		scanlines.displacementSpeed = 0.038f;
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
		musicMix.SetFloat("lowpassFreq", 22000);
		//GameMaster.me.decreaseCA();
		//Camera.main.GetComponent<PostProcessingBehaviour> ().enabled = false;
		GameMaster.me.RemoveSlowFX();
		scanlines.displacementSpeed = 0.525f;

	}


}
