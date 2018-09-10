using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flashable : MonoBehaviour {

	public SpriteRenderer sprite;
	public PlayerMovementController player;
	float flashTimer;
	public float flashLength;
	public Color defaultColor;

	// Use this for initialization
	void Start () {

		//sprite = GetComponentInChildren<SpriteRenderer> ();
		defaultColor = sprite.color;
		player = GetComponent<PlayerMovementController>();
		//player = sprite.gameObject.GetComponent<PlayerMovementController>();

	}
	
	// Update is called once per frame
	void Update () {

		float totalTimeslow = (player.mana + player.otherPlayer.mana);
		float percentageOfMana = player.mana / totalTimeslow;

		float a = (float)player.bulletTimer / player.bulletCooldown;
//		Debug.Log(a);

		//defaultColor = new Color(sprite.color.r, sprite.color.g, sprite.color.b, a);
		Color HSV;
		float h, s, v;
		Color.RGBToHSV(defaultColor, out h, out s, out v);
		HSV = Color.HSVToRGB(h, percentageOfMana * s + .25f, v);
		defaultColor = HSV;

		Flash ();

		
	}

	void OnCollisionEnter2D(Collision2D coll) {

		if (coll.gameObject.tag == "bullet" && !coll.gameObject.GetComponent<Bullet>().decayed) {

			flashTimer = flashLength;

		}
	
	}

	void Flash() {

			if (flashTimer > 0) {

			sprite.color = Random.ColorHSV(0f, 1f, 1f, 1f, 0.5f, 1f);

				flashTimer -= Time.deltaTime;


			} else {

				sprite.color = defaultColor;
			}
		}



	}



