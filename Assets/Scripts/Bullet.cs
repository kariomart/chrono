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

	public ParticleSystem hitObjectEffect;
	public ParticleSystem shoot;
	public ParticleSystem hitPlayerEffect;
	public ParticleSystem trail;
	public AudioClip playerHit;
	public AudioClip bounce;
	public AudioClip tick;

	public GameObject decayEffect;
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
		//decayColor = Color.grey;
		//ParticleSystem temp = Instantiate (shoot, transform.position, Quaternion.identity);
		//temp.gameObject.transform.parent = transform;

	}
	
	// Update is called once per frame
	void Update () {

		nonDecayedTime = Time.time - spawnTime;

	}

	void FixedUpdate() {

		//Debug.Log (rb.velocity);


		if (nonDecayedTime >= decayTime || decayed) {

			sprite.color = decayColor;
			decayEffect.SetActive (true);
			var main = middle.main;
			main.startColor = Color.magenta;
			//trail.gameObject.SetActive(false);
			decayed = true;

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



	}

	void OnCollisionEnter2D(Collision2D coll) {


		if (coll.gameObject.tag == "Player" && decayed) {

			SoundController.me.PlaySound (tick, 1f);
			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();
			player.amountOfBullets ++;
			Destroy (this.gameObject);

		}

		if (coll.gameObject.tag == "Player" && !decayed) {

			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();
			player.health -= dmg;
			//Camera.main.GetComponent<Screenshake> ().SetScreenshake (.25f, .15f * ((6f - coll.gameObject.GetComponent<PlayerMovementController>().health) / 2));
			Camera.main.GetComponent<Screenshake>().SetScreenshake(0.35f, .25f);
			GameObject flash = Instantiate (DamageFlash, transform.position, Quaternion.identity);
			flash.GetComponent<SpriteRenderer> ().color = coll.gameObject.GetComponentInChildren<SpriteRenderer> ().color;
			SoundController.me.PlaySound (playerHit, 1f);
			Instantiate (hitPlayerEffect, transform.position, Quaternion.identity);
			Destroy (flash, .025f); 
			Destroy (this.gameObject);

		}

		if ((coll.gameObject.tag == "Stage" || coll.gameObject.tag == "Wall" || coll.gameObject.tag == "Pinata") && coll.contacts.Length > 0) {
			
			vel = Geo.ReflectVect (prevVel.normalized, coll.contacts [0].normal) * (prevVel.magnitude * 0.65f);
			bounceCount++;

			if (bounceCount >= 4) {
				decayed = true;

			}

			SoundController.me.PlaySound (bounce, .2f);
			ParticleEffect (coll.gameObject);

		}

		if (coll.gameObject.tag == "bullet") {
			
			Debug.Log(coll.contacts.Length);
			if (coll.contacts.Length > 0) {
				vel = Geo.ReflectVect (prevVel.normalized, coll.contacts [0].normal) * (prevVel.magnitude * 0.65f);
			}
			bounceCount++;

			if (bounceCount >= 4) {
				decayed = true;
			}

			SoundController.me.PlaySound (bounce, .2f);
			ParticleEffect (coll.gameObject);


		}



		if (coll.gameObject.tag == "bullet") {

			//this.vel *= -.3f;
			SoundController.me.PlaySound (bounce, .2f);




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
}
