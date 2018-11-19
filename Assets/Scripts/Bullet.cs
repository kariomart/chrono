using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {


	public float defaultSpd;
	public float spd;
	public int dmg;
	public Rigidbody2D rb;
	public SpriteRenderer sprite;
	public BoxCollider2D pickupBox;
	float spawnTime;
	float nonDecayedTime;
	public float decayTime;
	public bool decayed;
	public float decayVel;
	public Vector2 vel;
	public float maxSpd;
	public int bounceCount;
	public float decayDeath;
	public float decayDeathCounter;
	public float lifetime;
	public bool slowZoneAccel;

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

	public ParticleSystem bulletWall;
	public ParticleSystem bulletSpawned;

	public Color decayColor = Color.grey;

	public GameObject DamageFlash;
	Vector2 prevVel;
	public float maxMapX;
	public float maxMapY;
	public bool slowzone;

	// Use this for initialization
	void Start () {

		rb = GetComponent<Rigidbody2D> ();
		sprite = GetComponent<SpriteRenderer> ();
		spawnTime = Time.time;
		defaultSpd = spd;
		if (lifetime == 0) {
			lifetime = Random.Range(3f, 5f);
		}

		//decayColor = Color.grey;
		//ParticleSystem temp = Instantiate (shoot, transform.position, Quaternion.identity);
		//temp.gameObject.transform.parent = transform;

	}
	
	// Update is called once per frame
	void Update () {

		nonDecayedTime = Time.time - spawnTime;

		if (decayed) {
			decayDeathCounter += Time.deltaTime;
//			Debug.Log(decayDeathCounter);


			if (decayDeathCounter > lifetime) {
				BulletManager.amtBullets --;
				Destroy(gameObject);
			}
		}

		if (lifetime - decayDeathCounter <= 8) {
			//blinking();
		}


		Color c = sprite.color;
//		Debug.Log((lifetime / decayDeathCounter) / 10);
		sprite.color = new Color(c.r, c.g, c.b, (lifetime / decayDeathCounter) / 10);


	}

	void FixedUpdate() {

		//Debug.Log (rb.velocity);

		if (nonDecayedTime >= decayTime || decayed) {

			//sprite.color = decayColor;
			//decayEffect.SetActive (true);
			var main = middle.main;
			main.startColor = decayColor;
			pickupBox.enabled = true;
			//trail.gameObject.SetActive(false);
			decayed = true;
			//sprite.enabled = true;
			//middle.Stop();
			trail.Stop();
		}

		if (Mathf.Abs(transform.position.x) > maxMapX) {
			if (transform.position.x > 0){
				transform.position = new Vector2(-transform.position.x + .25f, transform.position.y);
			} else {
				transform.position = new Vector2(-transform.position.x - .25f, transform.position.y);
			}
			//vel.x *= 1.1f;
			vel *= .8f;
		}

		if (Mathf.Abs(transform.position.y) > maxMapY) {
			if (transform.position.y > 0){
				transform.position = new Vector2(transform.position.x, -transform.position.y + .25f);
			} else {
				transform.position = new Vector2(transform.position.x, -transform.position.y - .25f);
			}
			//vel.x *= 1.1f;
			vel *= .8f;
		}

		if (spd < decayVel && !decayed && !slowzone) {
			decayed = true;
		}

		spd = Mathf.Clamp(spd, 0, maxSpd);

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
		
		if (coll.gameObject.layer == LayerMask.NameToLayer("SlowZone")) {

			slowzone = true;

			if ((GameMaster.me.player1.slow || GameMaster.me.player2.slow)) {
				spd *= 3f;	
			} else {
				spd *= .2f;	
			}
			decayed = false;
			trail.Play();
		}


			// if (!coll.GetComponent<SlowZone>().slow) {

			// 	spd *= 2f;
			// 	//Debug.Log("Slowzone");

			// } else {

			// 	spd *= .2f;
			// }


			//Debug.Log (rb.velocity);
			//Debug.Log ("trying to special case velocity");
			//vel *= Random.Range (-.1f, -.2f);


		
	}

	void OnTriggerStay2D(Collider2D coll) {

		if (coll.gameObject.layer == LayerMask.NameToLayer("SlowZone")) {

			if ((GameMaster.me.player1.slow || GameMaster.me.player2.slow)) { 
				slowZoneAccel = true;
			}
			decayed = false;
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

		if (coll.gameObject.layer == LayerMask.NameToLayer("SlowZone")) {
			if (slowZoneAccel) {
				spd *= 6f;
			} 
			//spd = defaultSpd;
			decayed = false;
			pickupBox.enabled = false;
			trail.Play();
		}

		slowzone = false;

	}

	void OnCollisionEnter2D(Collision2D coll) {

		//Debug.Log(decayed);

		if (coll.gameObject.tag == "Player" && decayed) {

			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();
//			Debug.Log(decayed);
			if (player.amountOfBullets < 9) {
				SoundController.me.PlaySound (tick, 1f, 1, transform.position.x);
				player.amountOfBullets ++;
				player.updateUI();
				Destroy (this.gameObject);
			}

		}


		else if (coll.gameObject.tag == "Player" && !decayed) {

			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();
//			Debug.Log(decayed);
			if (!player.invuln) {
	//			player.health -= dmg;
				if (player.health == 1) {
					SoundController.me.PlaySoundAtNormalPitch (lastHit, 1f);	
				} else {
					SoundController.me.PlaySoundAtNormalPitch (playerHit, 1f, transform.position.x);
					//Debug.Log("???");
				}
				player.respawn();
				GameMaster.me.addColorDrift();
				GameObject flash = Instantiate (DamageFlash, transform.position, Quaternion.identity);
				Camera.main.GetComponent<Screenshake>().SetScreenshake(0.35f, .25f, player);
				Destroy (this.gameObject);
				Destroy (flash, .020f); 

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
			//ParticleEffect (coll.gameObject);
			GameMaster.me.SpawnParticle(bulletWall, coll.contacts[0].point, Color.white, coll.gameObject.GetComponent<SpriteRenderer>().color);

		}

		if (coll.gameObject.tag == "bullet") {
			
//			Debug.Log(coll.contacts.Length);
			Bullet b = coll.gameObject.GetComponent<Bullet>();
			if (coll.contacts.Length > 0) {

				if (b.vel == Vector2.zero) {
					b.vel = vel;
					b.prevVel = vel;
					b.spd = spd;
					
				} else {
					vel = Geo.ReflectVect (prevVel.normalized, coll.contacts [0].normal) * (prevVel.magnitude * 0.65f);
					}
				}
			bounceCount++;

			if (bounceCount >= 4) {
				decayed = true;
			}


			playBounceSound();
			ParticleEffect (coll.gameObject);


		}






		if (coll.gameObject.tag == "Pinata") {


			Pinata pinata = coll.gameObject.GetComponent<Pinata> ();
			//pinata.StartCoroutine(pinata.Scale());
			pinata.health--;

			if (pinata.physics) {
				pinata.physics.vel += -vel * 5f;
			}

			pinata.Shrink();
//			pinata.gameObject.GetComponent<Animator> ().enabled = false;
			ParticleEffect (coll.gameObject);
		
		}
			
//		Debug.Log ("Collided with " + coll.gameObject.name);
		//Debug.Log (coll.gameObject.layer.ToString());
		//Destroy (this.gameObject);

	}

	void blinking() {

		ParticleSystem.MinMaxGradient gradient = new Gradient();
		GradientColorKey[] cK = new GradientColorKey[1];
		GradientAlphaKey[] aK = new GradientAlphaKey[1];

		//Debug.Log(c1 + " " + c2);		
		cK[0].color = Color.grey;
//		cK[1].time = 1;
		aK[0].alpha = Mathf.PingPong(Time.time, 1);
		Debug.Log(Mathf.PingPong(Time.time, 1));

		
		gradient.gradient.SetKeys(cK, aK);
		
		var main = middle.main;
		gradient.mode = ParticleSystemGradientMode.Gradient;
		//main.startColor.mode = ParticleSystemGradientMode.Gradient;

		main.startColor = gradient;  
		
	}

	void playBounceSound() {

		//float vol = Mathf.Clamp((float)bounceCount / 10f, .2f, .05f);
		//float vol = 1 / (float)bounceCount;
		float vol = 1;
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
