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
		Debug.Log(roundsNeeded);
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

		redScore = GameObject.Find ("redScore").GetComponent<TextMesh>();
		blueScore = GameObject.Find ("blueScore").GetComponent<TextMesh>();
		redSetsMesh = GameObject.Find ("redSets").GetComponent<TextMesh>();
		blueSetsMesh = GameObject.Find ("blueSets").GetComponent<TextMesh>();
		timeMaster = GameObject.Find("TimeManager").GetComponent<TimeManager>();

	}

	public void updateUI() {

		redScore.text = redWins.ToString();
		blueScore.text = blueWins.ToString();
		redSetsMesh.text = redSets + " ";
		blueSetsMesh.text = blueSets + " ";

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

}
