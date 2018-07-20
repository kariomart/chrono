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
	public int redWins;
	public int blueWins;
	public string winner;

	public TextMesh redScore;
	public TextMesh blueScore;

	bool gameLoaded;
	public string scene;




	// Use this for initialization
	void Start () {

		DontDestroyOnLoad (this.gameObject);


		if (me == null) {
			me = this;
		} else {
			Destroy (this.gameObject);
		}

		bestOf = 3;
		
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
		SoundController.me.PlaySound (playSoundEffect, .25f);
		UnityEngine.SceneManagement.SceneManager.LoadScene (scene);

	}

	public void dropdownItemSwitched() {

		SoundController.me.PlaySound (dropdownSoundEffect, .5f);
		int temp = GameObject.Find ("Dropdown").GetComponent<TMP_Dropdown> ().value;

		if (temp == 1) {
			bestOf = 5;
		} else if (temp == 2) {
			bestOf = 7;
		}
			


	}

	public void hoverSound() {

		SoundController.me.PlaySound (hoverSoundEffect, .5f);

	}

	public void findUI() {

		redScore = GameObject.Find ("redScore").GetComponent<TextMesh>();
		blueScore = GameObject.Find ("blueScore").GetComponent<TextMesh>();

	}

	public void updateUI() {

		redScore.text = redWins.ToString();
		blueScore.text = blueWins.ToString();

	}

}
