using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour {

	public TimeManager fatherTime;
	public PlayerMovementController otherPlayer;
	public TextMesh playerWon;

	public string rightTrigger;
	public string leftTrigger;
	public string xButton;
	public string aButton;
	//public string rightStick;
	public string leftStickH;
	public string leftStickV;
	public string shootKey;

	public string colorName;
	public Color playerColor;

	public float health;

    Vector2 vel;
	Vector2 bulletDir;
    bool jumpFlag;
    bool grounded;

	public bool right;
	public bool left;
	public bool slow;
	public bool speed;

    public Transform groundPt1;
    public Transform groundPt2;
	public Transform shootPt;
    public float jumpSpd;
    public float gravity;
    public float unjumpBonusGrav;
 	Rigidbody2D rb;

	int face;
    int jmpTimer;
    public float runMaxSpd;
    public float airMaxSpd;
    public float runAccel;
    public float airAccel;
    bool spinning;
    public Transform sprite;
    public float spinSpd;
    public float spinDir;
	public float knockbackAmount;


    Vector3 defSprScale;
    public GameObject bullet;
	public GameObject manaBar;
	public TextMesh healthBar;
	public GameObject reticle;
	public bool gameOver;
    public int shotCoolDown;
    int shotTimer;
    bool safety;
    public float bulletSpeed;
	public float slowdownModifier = 0.05f;
	public float slowdownLength = 2f;
	public float maxMana;
	public float mana; 
	public float timeManaDrain;
	public float bulletManaDrain;
	public int maxBullets = 5;
	public int amountOfBullets;

	public Vector2 defaultScale;
	public GameObject pivot;

	public bool timeSlowed;

	public Vector2 dir;
	public Vector2 defaultShootingDirection = new Vector2(1, 0);
    BoxCollider2D box;
    Vector2[] debugPts;

	public AudioClip shootSound;
	public AudioClip whoosh;

	public ParticleSystem shootEffect;
	public ParticleSystem jumpEffect;
	float scaleSpd;

	public GameObject letterbox;
	public GameObject camera;
    
    
	// Use this for initialization
	void Start () {
		
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        defSprScale = sprite.localScale;
        debugPts = new Vector2[2];
		defaultScale = pivot.transform.localScale;//new Vector2 (sprite.BroadcastMessagetransform.localScale.x, transform.localScale.y); 

	}
	
	// Update is called once per frame
	void Update () {



		right = Input.GetAxis (leftStickH) > 0;

		left = Input.GetAxis (leftStickH) < 0;



		if (Input.GetAxis (rightTrigger) > 0 && mana - timeManaDrain >= 0) {
			slow = true;
			mana -= timeManaDrain;
	

		} else {
			slow = false;
		}

		if (Input.GetAxis (leftTrigger) > 0 && mana - timeManaDrain >= 0) {
			speed = true;
			mana -= timeManaDrain;

		} else {
			speed = false;
		}

		dir = new Vector2 (Input.GetAxis (leftStickH), Input.GetAxis (leftStickV));
		dir.Normalize ();


		if (Input.GetButtonDown(aButton) || Input.GetKeyDown(KeyCode.Space)) {
			jumpFlag = true;
			Instantiate (jumpEffect, pivot.transform.position, Quaternion.identity);
		}


		if (Input.GetButtonDown (xButton) || Input.GetKeyDown(shootKey)) {

			if (amountOfBullets > 0) {
				amountOfBullets--;
				ShootBullet ();
			} else {
				// do sound effect / text effect here for no mana
			}

		}

		if (Input.GetButtonDown ("start")) {
				Time.timeScale = 1f;
				Application.LoadLevel(Application.loadedLevel);
		}

		manaBar.transform.localScale = new Vector2 (mana, manaBar.transform.localScale.y);

		if (healthBar != null) {
			healthBar.text = health.ToString ();
		}

		if (health <= 0 && !gameOver) {

			Destroy (healthBar.gameObject);
			gameOver = true;
			Instantiate (letterbox, new Vector2(transform.position.x, transform.position.y - 1f), Quaternion.identity);
			CamGameOver cameraController = camera.GetComponent<CamGameOver> ();
			camera.GetComponent<Screenshake> ().enabled = false;
			Debug.Log (transform.position.x + " " + transform.position.y + " " + -10);
			camera.transform.position = new Vector3 (transform.position.x, transform.position.y, -10);
			cameraController.playerLost = this.GetComponent<PlayerMovementController> ();
			cameraController.textColor = playerColor;
			cameraController.enabled = true;

			Destroy (this.gameObject, 1.2f);
			//camera.lookAtTarget = this.transform;
			//camera.moveToTarget = this.transform;
			//spawn letterbox


			//zoom camera
			//place text
			//explosion particle + speed shake
			//Time.timeScale = 0f;
			//playerWon.text = otherPlayer.gameObject.name + " has won!";

		}
			
	}

    private void FixedUpdate() {
       
		float yScale = defaultScale.y + (Mathf.Abs(vel.y) * 0.02f);
		scaleSpd += ((yScale - pivot.transform.localScale.y) * 6f);
		scaleSpd *= 5.4f * Time.fixedDeltaTime;

		if (!spinning) {
			pivot.transform.localScale = new Vector2 (defaultScale.x + (defaultScale.y - pivot.transform.localScale.y), pivot.transform.localScale.y + scaleSpd);
		}


        SetGrounded();

		if (!grounded && spinning) {
			sprite.eulerAngles = new Vector3(0, 0, sprite.eulerAngles.z + ((spinSpd) * Time.fixedDeltaTime * spinDir));
			//sprite.localScale = new Vector3(defSprScale.x, defSprScale.y * .75f, 1f);
		} else {
			sprite.eulerAngles = Vector3.zero;
			//sprite.localScale = defSprScale;
		}


		if (jumpFlag && grounded) {
			if ((left || right)) {
				spinning = true;

				if (left) spinDir = 1;
				if (right) spinDir = -1;
			}
			vel.y = jumpSpd;
		} else if (jumpFlag && !grounded) {
			safety = false;
		}
			

		float accel = runAccel;
        float mx = runMaxSpd;

        if (!grounded) {
			vel.y -= gravity * Time.fixedDeltaTime;
            accel = airAccel;
            mx = airMaxSpd;
        }
			
		if (vel.y > 0 && !Input.GetButton(aButton)) {
			vel.y -= unjumpBonusGrav * Time.fixedDeltaTime;
		}

        if (right && !slow) {
			vel.x += accel * Time.fixedDeltaTime;
			face = 1;

        }
        if (left && !slow) {
			vel.x -= accel * Time.fixedDeltaTime;
			face = -1;
        }
				

		vel.x = Mathf.Max(Mathf.Min(vel.x, mx), -mx);

        if (!right && !left) {
            vel.x = 0;
        }

        jumpFlag = false;
        shotTimer--;

		if (gameOver) {
			vel = Vector2.zero;
		}
		rb.MovePosition ((Vector2)transform.position + vel * Time.fixedDeltaTime);
		//bulletDir = Vector2.zero;

		 
	
		if (mana > maxMana)
			mana = maxMana;

		if (mana < 0)
			mana = 0;

		reticle.transform.position = new Vector2 (shootPt.transform.position.x + (dir.x * .5f), shootPt.transform.position.y + (dir.y * .5f));

    }

	void OnCollisionEnter2D(Collision2D coll) {

//		Debug.Log ("Collided with " + coll.gameObject.name);

		if (coll.gameObject.tag == "Stage") {
//			Debug.Log ("COLLIDED " + vel);
//
//			if (vel.y < -5) {
//
//			}
//
//			else if (vel.x > vel.y) {
//				vel = new Vector2 (vel.x * Random.Range (-1f, -2f), vel.y);
//			}
//
//			else if (vel.y > vel.x) {
//				vel = new Vector2 (vel.x, vel.y * Random.Range (-1f, -2f));
//			}
//				
//			//vel += (-dir * 3f);
//			//vel *= Random.Range(-1f, -3f);
//			Debug.Log ("AFTER " + vel);


		}
		if (coll.gameObject.tag == "Player") {
			vel.y = jumpSpd;
		}
		if (coll.gameObject.tag == "Bullet") {
			Bullet2 bull = coll.gameObject.GetComponent<Bullet2> ();
			vel = bull.vel * knockbackAmount;
		}

		if (coll.gameObject.name == "PINATA") {
			vel.y = jumpSpd;
		}
	}

	

	public void ShootBullet() {

		SoundController.me.PlaySound (shootSound, 1f, maxBullets / (amountOfBullets + 1));
		//SoundController.me.PlaySound(shootSound
//		Debug.Log ("sound triggered");
		GameObject tempBullet;
		if (dir.x == 0 && dir.y == 0) {

			tempBullet = Instantiate (bullet, new Vector3(shootPt.position.x + .5f, shootPt.transform.position.y + .5f), Quaternion.identity);
			tempBullet.GetComponent<Bullet2>().vel = defaultShootingDirection;

		} else {
			
		tempBullet = Instantiate (bullet, new Vector3(shootPt.transform.position.x + dir.x * .5f, shootPt.transform.position.y + dir.y * .5f), Quaternion.identity);
		tempBullet.GetComponent<Bullet2> ().vel = dir; 

		}
		mana -= bulletManaDrain;
	//	bulletDir =  tempBullet.GetComponent<Bullet2>().vel * knockbackAmount;
		SoundController.me.PlaySound (whoosh, 0.8f);

	}
		

    void SetGrounded() {
		Vector2 pt1 = transform.TransformPoint(box.offset + new Vector2(box.size.x / 2, -box.size.y / 2) + new Vector2(-.01f, 0));//(box.size / 2));
        Vector2 pt2 = transform.TransformPoint(box.offset - (box.size / 2) + new Vector2(.01f, 0));

        debugPts[0] = pt1;
        debugPts[1] = pt2;
		bool prevGrounded = grounded;
		grounded = Physics2D.OverlapArea(pt1, pt2, LayerMask.GetMask("Pinata")) != null;
        grounded = Physics2D.OverlapArea(pt1, pt2, LayerMask.GetMask("Platform")) != null;
//		Debug.Log (grounded);
		
        if (grounded) {
            spinning = false;
            vel.y = 0;
            safety = true;
			if (!prevGrounded) {
				scaleSpd = -.1f;
			}
        }
    }



}
