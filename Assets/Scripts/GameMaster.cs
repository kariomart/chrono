using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PostProcessing;
using Kino;
using Rewired;


public class GameMaster : MonoBehaviour {

	public static GameMaster me;
	public AudioClip hoverSoundEffect;
	public AudioClip playSoundEffect;
	public AudioClip dropdownSoundEffect;
	public int bestOf;
	public int roundsNeeded;
	public int setsNeeded;
	public int redWins;
	public int blueWins;
	public int redSets;
	public int blueSets;
	public string winner;
	public bool matchOver;
	public int amtBullets;
	public int amountOfLevels;

	public TextMesh redScore;
	public TextMesh blueScore;
	public TextMesh redSetsMesh;
	public TextMesh blueSetsMesh;
	public GameObject PauseMenu;
	public GameObject TimerUI;
	TextMesh TimerNum;
	
	public Sprite filledCircle;
	public Sprite filledSquare;
	public SpriteRenderer[] redScoreCircles = new SpriteRenderer[7];
	public SpriteRenderer[] blueScoreCircles = new SpriteRenderer[7];

	public SpriteRenderer[] redSetsSquares = new SpriteRenderer[2];
	public SpriteRenderer[] blueSetsSquares = new SpriteRenderer[2];


	bool gameLoaded;
	public string scene;

	public GameObject[] spawnPoints;
	public GameObject[] bulletSpawnPoints;
	public GameObject redPlayer;
	public GameObject bluePlayer;

	public TimeManager timeMaster;
	public GameObject player1_prefab;
	public GameObject player2_prefab;

	public PlayerMovementController player1;
	public PlayerMovementController player2;
	PostProcessingProfile retroFX_default;
	public AnalogGlitch glitchFX;


	public float fx_baseCA;
	public float fx_baseVignette;
	public float fx_slowVignette;

	public Player controller1;
	public Player controller2;

	public bool GameIsPaused;



	// Use this for initialization
	void Awake () {

//		DontDestroyOnLoad (this.gameObject);


		if (me == null) {
			me = this;
			
		} else {
			Destroy (this.gameObject);
		}

		//bestOf = 100;
		//roundsNeeded = (bestOf + 1) / 2;

	}

	public void initializeLevel(){ 

		roundsNeeded = 7;
		setsNeeded = 2;
		retroFX_default = Camera.main.GetComponent<PostProcessingBehaviour>().profile;
		glitchFX = Camera.main.GetComponent<AnalogGlitch>();
		setFXDefaults();
		SpawnPlayers();
		controller1 = ReInput.players.GetPlayer(0);
		controller2 = ReInput.players.GetPlayer(1);
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown (KeyCode.Escape)) {

			UnityEngine.SceneManagement.SceneManager.LoadScene ("menu");
//			if (me == null) {
//				me = this;
//			} else {
//				Destroy (this.gameObject);
//			}
		}

		if (matchOver && (controller1.GetButtonDown("Start") || controller2.GetButtonDown("Start"))) {
			UnityEngine.SceneManagement.SceneManager.LoadScene (pickRandomLevel());
		}
	}

	public void playPressed() {

//		Debug.Log(roundsNeeded);
		SoundController.me.PlaySound (playSoundEffect, .25f);
		//UnityEngine.SceneManagement.SceneManager.LoadScene (scene);
		string level = pickRandomLevel();
		UnityEngine.SceneManagement.SceneManager.LoadScene(level);

	}
	public IEnumerator ReEnablePlayer(GameObject obj, GameObject otherPlayer) {
		yield return new WaitForSeconds(.9f);
		Vector2 point = GameMaster.me.getFarthestSpawnPoint(otherPlayer.transform.position);
		obj.transform.position = point;
		obj.SetActive(true);
	}

	public void dropdownItemSwitched() {

		SoundController.me.PlaySound (dropdownSoundEffect, .5f);
		int temp = GameObject.Find ("Dropdown").GetComponent<TMP_Dropdown> ().value;

		if (temp == 1) {
			bestOf = 5;
		} else if (temp == 2) {
			bestOf = 100;
		}

	}

	public void hoverSound() {

		SoundController.me.PlaySound (hoverSoundEffect, .5f);

	}

	public string pickRandomLevel() {

		int rand = Random.Range(1, amountOfLevels + 1);
		return "FINAL_level" + rand;

	}

	public void addToScore(string player, int amt) {

		if (player == "red") {
			redWins += amt;
		}

		if (player == "blue") {
			blueWins += amt;
		}
	}

	public void findUI() {

		int count = 0;

		//redScore = GameObject.Find ("redScore").GetComponent<TextMesh>();
		//blueScore = GameObject.Find ("blueScore").GetComponent<TextMesh>();
		// redSetsMesh = GameObject.Find ("redSets").GetComponent<TextMesh>();
		// blueSetsMesh = GameObject.Find ("blueSets").GetComponent<TextMesh>();
		timeMaster = GameObject.Find("TimeManager").GetComponent<TimeManager>();
		
		redSetsSquares = GameObject.Find("redSets").GetComponentsInChildren<SpriteRenderer>();
		blueSetsSquares = GameObject.Find("blueSets").GetComponentsInChildren<SpriteRenderer>();

		redScoreCircles = GameObject.Find("redCircles").GetComponentsInChildren<SpriteRenderer>();
		blueScoreCircles = GameObject.Find("blueCircles").GetComponentsInChildren<SpriteRenderer>();
		PauseMenu = GameObject.Find("PauseMenu");

		TimerUI = GameObject.Find("Timer");
		TimerNum = TimerUI.GetComponentInChildren<TextMesh>(true);
		//Debug.Log(TimerNum);

		//PauseMenu = GameObject.Find("PauseMenu");

		// foreach(Transform c in redCircles.transform) {

		// 	redScoreCircles[count] = c.GetComponent<SpriteRenderer>();
		// 	count ++;

		// }

	}

	public void updateUI() {

//		redScore.text = redWins.ToString();
//		blueScore.text = blueWins.ToString();
		// redSetsMesh.text = redSets + " ";
		// blueSetsMesh.text = blueSets + " ";
		fillInScore("red", player2.health);
//		Debug.Log(player2.health);
		fillInScore("blue", player1.health);
		fillInSets("red", redSets);
		fillInSets("red", blueSets);

	}

	public void findSpawnPoints() {

		GameObject spawnPointParent = new GameObject("SpawnPoints");
		spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");

		foreach (GameObject s in spawnPoints) {
			s.transform.parent = spawnPointParent.transform;
			s.GetComponent<SpriteRenderer>().enabled = false;
		}

		GameObject bulletSpawnPointParent = new GameObject("BulletSpawnPoints");
		bulletSpawnPoints = GameObject.FindGameObjectsWithTag("BulletSpawner");

		foreach (GameObject b in bulletSpawnPoints) {
			b.transform.parent = bulletSpawnPointParent.transform;
			b.GetComponent<SpriteRenderer>().enabled = false;
		}
	}

	public Vector2 getFarthestSpawnPoint(Vector2 pos) {

		float maxDis = 0;
		Vector2 point = new Vector2();

		foreach (GameObject g in spawnPoints) {

			float dis = Vector2.Distance(pos, g.transform.position);
			//Debug.Log(pos + "\n" + g.transform.position + "\n" + dis);

			if (dis > maxDis) {
				maxDis = dis;
				point = g.transform.position;
			}
		}

		return point;

	}

	public void SpawnPlayers() {

		player1 = Instantiate(player1_prefab, LevelSettings.me.spawn1.position, Quaternion.identity).GetComponent<PlayerMovementController>();
		player2 = Instantiate(player2_prefab, LevelSettings.me.spawn2.position, Quaternion.identity).GetComponent<PlayerMovementController>();

		player1.amountOfBullets = 2;
		player2.amountOfBullets = 2;

		
	}

	public IEnumerator rumble(PlayerMovementController p, float strength, float length) {

		p.player.SetVibration(p.playerId, strength);
		yield return new WaitForSeconds(length);
		p.player.SetVibration(p.playerId, 0);

	}

	public IEnumerator Countdown(int seconds) {
		Time.timeScale = 0f;
		GameIsPaused = true;
        int count = seconds;
       // Debug.Log("A " + count);
		enableChildren(PauseMenu.transform, false);
		enableChildren(TimerUI.transform, true);

        for(count = seconds; count > 0; count --) {
           
			TimerNum.text = "" + (count);
            yield return new WaitForSecondsRealtime(1);
        }
       
        // count down is finished...
       	Resume();
    }

	public void Pause() {

		//pauseMenu.SetActive(true)
		timeMaster.globalTimescale = Time.timeScale;
		Time.timeScale = 0f;
		GameIsPaused = true;
		enableChildren(PauseMenu.transform, true);

	}


	public void Resume() {

		//pauseMenu.SetActive(false)
		TimerUI.SetActive(false);
		GameIsPaused = false;
		Time.timeScale = timeMaster.globalTimescale;
		
	}

	void enableChildren(Transform o, bool active) {


		if (active) {

			foreach(Transform c in o) {
				c.gameObject.SetActive(true);
			}
		}

		if (!active) {

			foreach(Transform c in o) {
				c.gameObject.SetActive(false);
			}
		}

	}

	void fillInScore(string player, int health) {

		if (player == "red") {

			int circlesToFill = 7 - health;

			for (int i = 0; i < circlesToFill; i++) {

				redScoreCircles[i].sprite = filledCircle;
			}
		}

		if (player == "blue") {

			int circlesToFill = 7 - health;

			for (int i = 0; i < circlesToFill; i++) {

				blueScoreCircles[i].sprite = filledCircle;
			}
		}



	}


	void fillInSets(string player, int sets) {

		if (player == "red") {

			int circlesToFill = sets;

			for (int i = 0; i < circlesToFill; i++) {

				redSetsSquares[i].sprite = filledSquare;
			}
		}

		if (player == "blue") {

			int circlesToFill = sets;

			for (int i = 0; i < circlesToFill; i++) {

				blueSetsSquares[i].sprite = filledSquare;
			}
		}



	}


	public void AddSlowFX() {

		//addMotionBlur();
		increaseCA();
		increaseVignette();
		glitchFX.colorDrift += 0.0005f;

	}

	public void RemoveSlowFX() {

		//removeMotionBlur();
		decreaseCA();
		decreaseVignette();
		glitchFX.colorDrift = 0f;

	}

	public void addMotionBlur() {

		retroFX_default.motionBlur.enabled = true;
	}

	public void removeMotionBlur() {

		retroFX_default.motionBlur.enabled = false;

	}

	public void addColorDrift() {

		glitchFX.colorDrift = 1;

	} 

	public void removeColorDrift() {

		glitchFX.colorDrift = 0;

	}

	public void increaseVignette() {
		var v = retroFX_default.vignette.settings;
		if (v.intensity < fx_slowVignette) {
			v.intensity += .005f;
		}
		retroFX_default.vignette.settings = v;
	} 

	public void decreaseVignette() {
		var v = retroFX_default.vignette.settings;
		if (v.intensity > fx_baseVignette) {
			v.intensity -= .01f;
		}
		retroFX_default.vignette.settings = v;
	} 

	public void increaseCA() {

		var ca = retroFX_default.chromaticAberration.settings;
		ca.intensity += 0.005f;
		retroFX_default.chromaticAberration.settings = ca;

	}

	public void decreaseCA() {

		var ca = retroFX_default.chromaticAberration.settings;
		//ca.intensity = fx_baseCA;
		if (ca.intensity > fx_baseCA) {
			ca.intensity -= 0.01f;
			}
		retroFX_default.chromaticAberration.settings = ca;

	}

	public void setFXDefaults() {

		var ca = retroFX_default.chromaticAberration.settings;
		ca.intensity = fx_baseCA;

		retroFX_default.chromaticAberration.settings = ca;

		var v = retroFX_default.vignette.settings;
		v.intensity = fx_baseVignette;

		retroFX_default.vignette.settings = v;
	
	}

	public void SpawnParticle(ParticleSystem fx, Vector2 pos) {

		Instantiate(fx.gameObject, pos, Quaternion.identity);

	}

	public void SpawnParticle(ParticleSystem fx, Vector2 pos, Color c) {

		ParticleSystem p = Instantiate(fx.gameObject, pos, Quaternion.identity).GetComponent<ParticleSystem>();
		var main = p.main;
		main.startColor = c;

	}

	public void SpawnParticle(ParticleSystem fx, Vector2 pos, Color c1, Color c2) {

		ParticleSystem p = Instantiate(fx.gameObject, pos, Quaternion.identity).GetComponent<ParticleSystem>();
		ParticleSystem.MinMaxGradient gradient = new Gradient();
		GradientColorKey[] cK = new GradientColorKey[2];
		GradientAlphaKey[] aK = new GradientAlphaKey[1];

		//Debug.Log(c1 + " " + c2);		
		cK[0].color = c1;
		cK[1].color = c2;
		cK[1].time = 1;
		aK[0].alpha = 1f;

		
		gradient.gradient.SetKeys(cK, aK);
		
		var main = p.main;
		gradient.mode = ParticleSystemGradientMode.Gradient;
		//main.startColor.mode = ParticleSystemGradientMode.Gradient;

		main.startColor = gradient;  

	}


}
