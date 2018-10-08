using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

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

	public TextMesh redScore;
	public TextMesh blueScore;
	public TextMesh redSetsMesh;
	public TextMesh blueSetsMesh;

	public Sprite filledCircle;
	public Sprite filledSquare;
	public SpriteRenderer[] redScoreCircles = new SpriteRenderer[7];
	public SpriteRenderer[] blueScoreCircles = new SpriteRenderer[7];

	public SpriteRenderer[] redSetsSquares = new SpriteRenderer[3];
	public SpriteRenderer[] blueSetsSquares = new SpriteRenderer[3];


	bool gameLoaded;
	public string scene;

	public GameObject[] spawnPoints;
	public GameObject[] bulletSpawnPoints;
	public GameObject redPlayer;
	public GameObject bluePlayer;

	public TimeManager timeMaster;





	// Use this for initialization
	void Start () {

		DontDestroyOnLoad (this.gameObject);


		if (me == null) {
			me = this;
			
		} else {
			Destroy (this.gameObject);
		}

		//bestOf = 100;
		//roundsNeeded = (bestOf + 1) / 2;
		roundsNeeded = 7;
		setsNeeded = 3;
		

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
	}

	public void playPressed() {

		roundsNeeded = (bestOf + 1) / 2;
//		Debug.Log(roundsNeeded);
		SoundController.me.PlaySound (playSoundEffect, .25f);
		UnityEngine.SceneManagement.SceneManager.LoadScene (scene);

	}
	public IEnumerator ReEnablePlayer(GameObject obj) {
		yield return new WaitForSeconds(.6f);
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
		fillInScore("red", timeMaster.player1.health);
		fillInScore("blue", timeMaster.player2.health);
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

}
