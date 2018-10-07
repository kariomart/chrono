using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {


	public float defaultSpd;
	public float spd;
	public int dmg;
	public Rigidbody2D rb;
	public SpriteRenderer sprite;
	float spawnTime;
	float nonDecayedTime;
	public float decayTime;
	public bool decayed;
	public Vector2 vel;
	public int bounceCount;
	public float decayDeath;
	public float decayDeathCounter;
	public float lifetime;

	public ParticleSystem hitObjectEffect;
	public ParticleSystem shoot;
	public ParticleSystem hitPlayerEffect;
	public ParticleSystem trail;
	public AudioClip playerHit;
	public AudioClip lastHit;
	public AudioClip bounce1;
	public AudioClip bounce2;
	public AudioClip bounce3;

	public AudioClip tick;


	public GameObject decayEffect;
	public ParticleSystem bulletCore;
	public ParticleSystem middle;

	public Color decayColor = Color.grey;

	public GameObject DamageFlash;
	Vector2 prevVel;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody2D> ();
		sprite = GetComponent<SpriteRenderer> ();
		spawnTime = Time.time;
		defaultSpd = spd;
		if (lifetime == 0)
			lifetime = Random.Range(3f, 5f);
		//decayColor = Color.grey;
		//ParticleSystem temp = Instantiate (shoot, transform.position, Quaternion.identity);
		//temp.gameObject.transform.parent = transform;

	}
	
	// Update is called once per frame
	void Update () {

		nonDecayedTime = Time.time - spawnTime;

		if (decayed) {
			decayDeathCounter += Time.deltaTime;


			if (decayDeathCounter > lifetime) {
				Destroy(gameObject);
			}
		}
		



	}

	void FixedUpdate() {

		//Debug.Log (rb.velocity);

		if (nonDecayedTime >= decayTime || decayed) {

			sprite.color = decayColor;
			//decayEffect.SetActive (true);
			var main = middle.main;
			main.startColor = decayColor;
			//trail.gameObject.SetActive(false);
			decayed = true;
			trail.Stop();

		}

		rb.MovePosition ((Vector2)transform.position + vel * spd * Time.fixedDeltaTime);
		prevVel = vel;

	}

	void ParticleEffect(GameObject obj) {

		Instantiate (hitObjectEffect, transform.position, Quaternion.identity);
		hitObjectEffect.startColor = Color.grey;

		if (obj.GetComponent<SpriteRenderer> () != null) {

//			Debug.Log (obj.GetComponent<SpriteRenderer> ().color);
			hitObjectEffect.startColor = obj.GetComponent<SpriteRenderer> ().color;

		} else {
			
			if (obj.GetComponentInChildren<SpriteRenderer> () != null) {

				Debug.Log ("Got sprite renderer in child!");
				hitObjectEffect.startColor = obj.GetComponent<SpriteRenderer> ().color;

			}

		}
	}
			



	void OnTriggerEnter2D(Collider2D coll) {

		if (coll.gameObject.tag == "Stage") {

			//Debug.Log (rb.velocity);
			//Debug.Log ("trying to special case velocity");
			//vel *= Random.Range (-.1f, -.2f);

		}

		if (coll.gameObject.tag == "pivot" && !decayed) {

			GameMaster.me.timeMaster.gameOverSlow = true;
			GameMaster.me.timeMaster.pos = coll.gameObject.transform.position;
		}
	}

	void OnTriggerExit2D(Collider2D coll) {

		if (coll.gameObject.tag == "pivot" && !decayed) {

			GameMaster.me.timeMaster.gameOverSlow = false;
		}

	}

	void OnCollisionEnter2D(Collision2D coll) {


		if (coll.gameObject.tag == "Player" && decayed) {

			SoundController.me.PlaySound (tick, .2f, 1, transform.position.x);
			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();
			player.amountOfBullets ++;
			player.updateUI();
			Destroy (this.gameObject);

		}

		if (coll.gameObject.tag == "Player" && !decayed) {

			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();

			if (!player.invuln) {
	//			player.health -= dmg;
				if (player.health == 1) {
					SoundController.me.PlaySoundAtNormalPitch (lastHit, 1f);	
				} else {
					SoundController.me.PlaySoundAtNormalPitch (playerHit, 1f, transform.position.x);
					//Debug.Log("???");
				}
				player.respawn();
				GameObject flash = Instantiate (DamageFlash, transform.position, Quaternion.identity);
				Camera.main.GetComponent<Screenshake>().SetScreenshake(0.35f, .25f);
				Destroy (this.gameObject);
				Destroy (flash, .025f); 

			} else {
				if (coll.contacts.Length > 0) {
					vel = Geo.ReflectVect (prevVel.normalized, coll.contacts [0].normal) * (prevVel.magnitude * 0.65f);
				}
			}

		}


		if ((coll.gameObject.tag == "Stage" || coll.gameObject.tag == "Wall" || coll.gameObject.tag == "Pinata") && coll.contacts.Length > 0) {
			
			vel = Geo.ReflectVect (prevVel.normalized, coll.contacts [0].normal) * (prevVel.magnitude * 0.65f);
			bounceCount++;

			if (bounceCount >= 4) {
				decayed = true;

			}

			playBounceSound();
			//SoundController.me.PlaySound (bounce1, .2f, Mathf.Clamp(bounceCount, 1, 3f), Mathf.Clamp(transform.position.x, -1, 1));
			ParticleEffect (coll.gameObject);

		}

		if (coll.gameObject.tag == "bullet") {
			
//			Debug.Log(coll.contacts.Length);
			if (coll.contacts.Length > 0) {
				vel = Geo.ReflectVect (prevVel.normalized, coll.contacts [0].normal) * (prevVel.magnitude * 0.65f);
			}
			bounceCount++;

			if (bounceCount >= 4) {
				decayed = true;
			}


			playBounceSound();
			ParticleEffect (coll.gameObject);


		}






		if (coll.gameObject.layer == LayerMask.NameToLayer("Pinata")) {

			Pinata pinata = coll.gameObject.GetComponent<Pinata> ();
			pinata.health--;
			pinata.gameObject.GetComponent<Animator> ().enabled = false;
			ParticleEffect (coll.gameObject);
		
		}
			
//		Debug.Log ("Collided with " + coll.gameObject.name);
		//Debug.Log (coll.gameObject.layer.ToString());
		//Destroy (this.gameObject);

	}

	void playBounceSound() {

		//float vol = Mathf.Clamp((float)bounceCount / 10f, .2f, .05f);
		float vol = .2f / (float)bounceCount;
//		Debug.Log(vol);

			if (bounceCount == 1) {
				SoundController.me.PlaySound (bounce1, vol, 1, transform.position.x);	
			} else if (bounceCount == 2) {
				SoundController.me.PlaySound (bounce2, vol, 1, transform.position.x);	
			} else if (bounceCount >= 3) {
				SoundController.me.PlaySound (bounce3, vol, 1, transform.position.x);	
			}


		}


}
