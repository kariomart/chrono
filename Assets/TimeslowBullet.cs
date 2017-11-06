using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeslowBullet : MonoBehaviour {


	public float spd;
	public int dmg;
	public float slowdownModifier;
	public Rigidbody2D rb;
	public SpriteRenderer sprite;
	public Vector2 vel;
	public int bounceCount;


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


	}

	// Update is called once per frame
	void Update () {


	}

	void FixedUpdate() {


		rb.MovePosition ((Vector2)transform.position + vel * spd * Time.fixedDeltaTime);
		prevVel = vel;

	}
		


	void OnTriggerEnter2D (Collider2D coll)
	{

		if (coll.gameObject.tag == "Stage") {


			vel *= -1;

		}

		if (coll.gameObject.tag == "bullet") {

			coll.gameObject.GetComponent<Bullet>().spd *= slowdownModifier;

		}

		if (coll.gameObject.tag == "Player") {

			coll.gameObject.GetComponent<PlayerMovementController> ().vel *= slowdownModifier;

		}

	}


	void OnTriggerExit2D (Collider2D coll)
	{

		if (coll.gameObject.tag == "Stage") {


		}

		if (coll.gameObject.tag == "bullet") {

			Bullet temp = coll.gameObject.GetComponent<Bullet> ();
			temp.spd = temp.defaultSpd;

		}

	}
	void OnCollisionEnter2D(Collision2D coll) {



//		if (coll.gameObject.tag == "Player" && decayed) {
//
//			SoundController.me.PlaySound (tick, 1f);
//			PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController> ();
//			player.amountOfBullets++;
//			Destroy (this.gameObject);
//
//		}




	}



}
