using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pinata : MonoBehaviour {

	public float health;
	public float startingHealth;
	bool exploded;
	public GameObject bullet;
	public float explosionSpeed;

	public ParticleSystem confetti;
	public AudioClip pop;
	public bool releaseActiveBullets;

	public int minBulletsToRelease;
	public int maxBulletsToRelease;

	public float scale;

	public float shakeIntensity;
	public float shakeDuration;

	// Use this for initialization
	void Start () {

		startingHealth = health;
		GrowAnimation ();
		//minBulletsToRelease = 1;
		//maxBulletsToRelease = 3;

	}
	
	// Update is called once per frame
	void Update () {

		if (health <= 0 && !exploded) {
			exploded = true;

			for (int i = 1; i < Random.Range (minBulletsToRelease, maxBulletsToRelease); i++) {
				
				GameObject tempBullet = Instantiate (bullet, new Vector2 (transform.position.x + Random.Range (-1, 1), transform.position.y + Random.Range (-1, 1)), Quaternion.identity);
				Bullet bulletTemp = tempBullet.GetComponent<Bullet> ();

				if (!releaseActiveBullets) {
					bulletTemp.decayColor = new Color (Random.Range (0f, 1f), Random.Range (0f, 1f), Random.Range (0f, 1f));
					bulletTemp.decayed = true;
				}

				bulletTemp.vel = new Vector2 (Random.Range (-explosionSpeed, explosionSpeed), Random.Range (-explosionSpeed, explosionSpeed));

			}

			Camera.main.GetComponent<Screenshake> ().SetScreenshake (shakeIntensity, shakeDuration);
			SoundController.me.PlaySoundAtNormalPitch (pop, 1, transform.position.x);
			Instantiate (confetti, transform.position, Quaternion.identity);
			Destroy (this.gameObject);

		}
			
		transform.localScale = new Vector2(2.1f, 2.1f) * (health / startingHealth * scale);

		
	}

	void OnTriggerEnter2D(Collider2D coll) {

//		Debug.Log ("pinata hit");
		if (coll.gameObject.tag == "bullet") {

			//health -= 1;

		}


	}

	void GrowAnimation() {

	}


}
