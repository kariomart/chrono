using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CamGameOver : MonoBehaviour {

	public Camera cam;
	public PlayerMovementController playerWon;
	public Color textColor;
	public GameObject explosionFX;

	public float zoomMin;
	public float startingZoom;
	public float zoomSpeed;

	public GameObject deathConfetti; 
	public GameObject letterbox;
	public GameObject letterboxObj;
	public GameObject winnerText;

	public AudioClip pop;
	public AudioClip fireworks;
	public AudioClip slam;

	public int winnerTextTime;
	public int explosionSoundTime;
	public int fireworksSoundTime;

	bool fireworksSound;
	bool winnerTextDisplayed;
	bool explosionSound;

	int counter = 0;

	[Range(0f, 3f)]
	public float firstPause;

	[Range(0f, 3f)]
	public float secondPause;

	// Use this for initialization
	void Start () {

		cam = GetComponent<Camera> ();
		startingZoom = cam.orthographicSize;
		//Instantiate (deathConfetti, new Vector3(transform.position.x, transform.position.y, -3), Quaternion.identity);

		if (playerWon.colorName == "red") {
			GameMaster.me.redSets ++;
			GameMaster.me.player1.rb.isKinematic = false;

		} if (playerWon.colorName == "blue") {
			GameMaster.me.blueSets++;
			GameMaster.me.player2.rb.isKinematic = false; 
		}

		StartCoroutine("gameOver");
		
	}

	IEnumerator gameOver() {

		PlayerMovementController p = playerWon.otherPlayer;
		cam.orthographicSize = 5;
		GameObject efx = Instantiate(explosionFX, p.transform.position + (Vector3.down * .35f), Quaternion.identity);
		letterboxObj = Instantiate (letterbox, new Vector2(transform.position.x, transform.position.y - .5f), Quaternion.identity);
		yield return new WaitForSeconds(firstPause);



		while (cam.orthographicSize > zoomMin) {
			cam.orthographicSize -= Time.deltaTime * zoomSpeed;
			//Debug.Log(Easing.EaseInQuad(startingZoom, zoomMin, zoomSpeed));
			yield return null;
		}


		this.gameObject.GetComponent<Screenshake> ().enabled = true;
		this.gameObject.GetComponent<Screenshake> ().defaultCameraPos = transform.position;
		this.gameObject.GetComponent<Screenshake> ().SetScreenshake (1.5f, 0.8f);

		Destroy(efx);
		SoundController.me.PlaySound (pop, 1f);
		p.sprite.gameObject.GetComponent<SpriteRenderer>().enabled = false;
		Instantiate(p.hitParticle, p.transform.position + (Vector3.down * .35f), Quaternion.identity);

		yield return new WaitForSeconds(secondPause);
		SoundController.me.PlaySound (slam, 1f);
		this.gameObject.GetComponent<Screenshake> ().SetScreenshake (1.5f, 0.8f);
		GameMaster.me.hideUI();
		GameObject w = Instantiate (winnerText.gameObject, (p.transform.position - (Vector3.forward * 6) - (Vector3.up * 1.35f)) , Quaternion.identity);
		TextMeshPro wt = w.GetComponentInChildren<TextMeshPro>();
		wt.text = playerWon.colorName + " wins";
		wt.color = playerWon.playerColor;
		yield return new WaitForSeconds(2f);
		CycleGame();



	}
	
	
	// Update is called once per frame
	void Update () {

	// 	counter++;
	// 	if (cam.orthographicSize > zoomMin) {
	// 		cam.orthographicSize -= Mathf.Pow (zoomSpeed, 2);
	// 	}

	// 	if (counter > winnerTextTime && !winnerTextDisplayed) {

	// 		TextMesh winner = Instantiate (winnerText, new Vector3 (transform.position.x, transform.position.y + .3f, -8), Quaternion.identity);
	// 		winner.text = playerWon.colorName + "\nhas won";
	// 		winner.color = playerWon.playerColor;
	// 		GameMaster.me.updateUI ();
	// 		SoundController.me.PlaySound (slam, 1f);
	// 		this.gameObject.GetComponent<Screenshake> ().SetScreenshake (1.5f, 0.8f);
	// 		winnerTextDisplayed = true;

	// 	}

	// 	if (counter > explosionSoundTime && !explosionSound) {

	// 		this.gameObject.GetComponent<Screenshake> ().enabled = true;
	// 		this.gameObject.GetComponent<Screenshake> ().defaultCameraPos = transform.position;
	// 		this.gameObject.GetComponent<Screenshake> ().SetScreenshake (1.5f, 0.8f);
	// 		SoundController.me.PlaySound (pop, 1f);
	// 		explosionSound = true;

	// 	}

	// 	if (counter > fireworksSoundTime && !fireworksSound) {

	// 		SoundController.me.PlaySound (fireworks, 1f);
	// 		fireworksSound = true;

	// 	}

	// 	bool redWon = GameMaster.me.redSets >= GameMaster.me.setsNeeded;
	// 	bool blueWon = GameMaster.me.blueSets >= GameMaster.me.setsNeeded;

	// 	if (redWon || blueWon) {
	// 		GameMaster.me.matchOver = true;
	// 	}

	// 	if ((counter > fireworksSoundTime + 150) && (redWon || blueWon)) {

	// 		GameMaster.me.winner = playerWon.colorName;
	// 		Time.timeScale = 1f;
	// 		cam.transform.position = new Vector3(0, 2.42f, -10);
	// 		Destroy(letterbox);
	// 		cam.orthographicSize = startingZoom;
	// 		GameMaster.me.enableMatchOver();
	// 		this.enabled = false;
	// 		//UnityEngine.SceneManagement.SceneManager.LoadScene ("gameover2");


	// 	}
		
 	}

	 void CycleGame() {


		bool redWon = GameMaster.me.redSets >= GameMaster.me.setsNeeded;
		bool blueWon = GameMaster.me.blueSets >= GameMaster.me.setsNeeded;

		if (redWon || blueWon) {
			GameMaster.me.matchOver = true;
		}

		if (redWon || blueWon) {

			GameMaster.me.winner = playerWon.colorName;
			Time.timeScale = 1f;
			cam.transform.position = new Vector3(0, 2.42f, -10);
			Destroy(letterboxObj);
			cam.orthographicSize = startingZoom;
			GameMaster.me.enableMatchOver();
			GameMaster.me.updateUI();
			this.enabled = false;
	 	}

		 GameMaster.me.roundOver = true;
	 }


}
