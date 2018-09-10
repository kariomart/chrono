using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController_OLD : MonoBehaviour {

	public TimeManager fatherTime;
	public PlayerMovementController otherPlayer;
	public TextMesh playerWon;
	public string playerId;

	public string rightTrigger;
	public string leftTrigger;
	public string xButton;
	public string yButton;
	public string aButton;
	//public string rightStick;
	public string leftStickH;
	public string leftStickV;
	public string shootKey;

	public string colorName;
	public Color playerColor;

	public float health;

    public Vector2 vel;
	Vector2 bulletDir;
	Vector2 prevVel;
	Vector2 walljumpDir;
    bool jumpFlag;
    bool grounded;
	int groundedCounter;
	bool onWall;

	public bool right;
	public bool left;
	public bool down;
	public bool slow;
	public bool speed;
	public bool fastfall;
	public bool crouching;

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
	public float wallJumpRange;
	public float crouchScale;



    Vector3 defSprScale;
    public GameObject bullet;
	//public GameObject timeSlowBullet;
	public GameObject manaBar;
	public TextMesh healthBar;
	public TextMesh bulletAmount;
	public GameObject reticle;
	public bool gameOver;
    public int shotCoolDown;
    int shotTimer;
    bool safety;
    public float bulletSpeed;
	public float bulletCooldown;
	public float bulletTimer;
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
	public Vector2 prevDir;
    BoxCollider2D box;
    Vector2[] debugPts;

	public AudioClip shootSound;
	public AudioClip whoosh;

	public ParticleSystem shootEffect;
	public ParticleSystem jumpEffect;
	float scaleSpd;

	public GameObject letterbox;
	public GameObject camera;
    public float groundDrag;
    
	// Use this for initialization
	void Start () {
		
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        defSprScale = sprite.localScale;
        debugPts = new Vector2[2];
		defaultScale = pivot.transform.localScale;//new Vector2 (sprite.BroadcastMessagetransform.localScale.x, transform.localScale.y); 

	}
	
	void Update () {

		right = Input.GetAxis (leftStickH) > 0;
		left = Input.GetAxis (leftStickH) < 0;
		down = ((Vector2.Dot(dir, new Vector2(0, -1)) > .7f));

		bulletTimer += Time.deltaTime;

		if (Input.GetAxis (rightTrigger) > 0 && mana - timeManaDrain >= 0) {
			slow = true;
			mana -= timeManaDrain;
			otherPlayer.mana += timeManaDrain;
	

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
		
		if (dir != Vector2.zero) {
			prevDir = dir;
		}


		if (Input.GetButtonDown(aButton) || Input.GetKeyDown(KeyCode.Space)) {
			jumpFlag = true;
			Instantiate (jumpEffect, pivot.transform.position, Quaternion.identity);
		}


		if (Input.GetButtonDown (xButton) || Input.GetKeyDown(shootKey)) {

			if (amountOfBullets > 0 && bulletTimer > bulletCooldown) {
				amountOfBullets--;
				ShootBullet ();
				bulletTimer = 0;
			} else {
				// do sound effect / text effect here for no mana
				}
		}

		// if (Input.GetButtonDown (yButton)) {
			
		// 	ShootSlowdownBullet ();
		// }


		if ((Input.GetButtonDown ("start") && otherPlayer.gameOver && !GameMaster.me.matchOver) || Input.GetKeyDown(KeyCode.R)) {
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
//			Debug.Log (transform.position.x + " " + transform.position.y + " " + -10);
			camera.transform.position = new Vector3 (transform.position.x, transform.position.y, -10);
			cameraController.playerLost = this.GetComponent<PlayerMovementController> ();
			cameraController.textColor = playerColor;
			cameraController.enabled = true;

			Destroy (this.gameObject, 1.2f);
			//this.sprite.GetComponent<SpriteRenderer>().enabled = false;
			//camera.lookAtTarget = this.transform;
			//camera.moveToTarget = this.transform;
			//spawn letterbox


			//zoom camera
			//place text
			//explosion particle + speed shake
			//Time.timeScale = 0f;
			//playerWon.text = otherPlayer.gameObject.name + " has won!";

		}

		updateBulletUI ();
			
	}

    private void FixedUpdate() {
       
		float yScale = defaultScale.y + (Mathf.Abs(vel.y) * 0.02f);
		scaleSpd += ((yScale - pivot.transform.localScale.y) * 6f);
		scaleSpd *= 5.4f * Time.fixedDeltaTime;

		if (!spinning) {
			pivot.transform.localScale = new Vector2 (defaultScale.x + (defaultScale.y - pivot.transform.localScale.y), pivot.transform.localScale.y + scaleSpd);
		}


        SetGrounded();
		wallCast();

//		Debug.Log(fastfall);
		if (down && vel.y < 0 && !fastfall) {
				fastfall = true;
				Debug.Log("test");
			}

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
				transform.localScale = defaultScale;

				if (left) spinDir = 1;
				if (right) spinDir = -1;
			}
			vel.y = jumpSpd;
		}

		if (fastfall) {
//			Debug.Log(100 * Time.fixedDeltaTime);
			vel.y *= (100 * Time.fixedDeltaTime);
		}
			
		// } else if (jumpFlag && !grounded) {
		// 	safety = false;
		//} 
		
		else if (onWall && jumpFlag) {
			spinning = false;
//			Debug.Log(-walljumpDir.x * 5);
			vel.x = -walljumpDir.x * 5;
			vel.y = jumpSpd;
			onWall = false;
		}
			

		float accel = runAccel;
        float mx = runMaxSpd;

        if (!grounded) {
			vel.y -= gravity * Time.fixedDeltaTime;
            accel = airAccel;
            mx = airMaxSpd;

			//Debug.Log(Vector2.Dot(dir, new Vector2(0, -1)));
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
				

		vel.x = Mathf.Clamp(vel.x, -mx, mx);//Mathf.Max(Mathf.Min(vel.x, mx), -mx);

        /*if (grounded && !right && !left) {
            vel.x = 0;
        }*/

		if (!left && !right && grounded){
			if (Mathf.Abs(vel.x) < groundDrag * Time.fixedDeltaTime) vel.x = 0;
			else vel.x -= (groundDrag * Mathf.Sign(vel.x)) * Time.fixedDeltaTime;
		}

        jumpFlag = false;
		//onWall = false;
        shotTimer--;

		if (gameOver) {
			vel = Vector2.zero;
		}

		prevVel = rb.position;
		rb.MovePosition ((Vector2)transform.position + vel * Time.fixedDeltaTime);

		if (vel.y != 0) {
//			Debug.Log(vel.y);
		}
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
			Bullet bull = coll.gameObject.GetComponent<Bullet> ();
			vel = bull.vel * knockbackAmount;
		}

		if (coll.gameObject.tag == "Pinata") {
//			Debug.Log("pinata");
			vel.y = jumpSpd;
		}
	}

	

	public void ShootBullet() {

		SoundController.me.PlaySound (shootSound, 1f, maxBullets / (amountOfBullets + 1));
		GameObject tempBullet;

		if (dir.x == 0 && dir.y == 0) {

			tempBullet = Instantiate (bullet, new Vector3(shootPt.position.x + prevDir.x * .5f, shootPt.transform.position.y + prevDir.y * .5f), Quaternion.identity);
			tempBullet.GetComponent<Bullet>().vel = prevDir;

		} else {
			
		tempBullet = Instantiate (bullet, new Vector3(shootPt.transform.position.x + dir.x * .5f, shootPt.transform.position.y + dir.y * .5f), Quaternion.identity);
		tempBullet.GetComponent<Bullet> ().vel = dir; 

		}

		SoundController.me.PlaySound (whoosh, 0.8f);

	}

	public void wallCast() {

		Ray2D myRay = new Ray2D(transform.position, vel);
		RaycastHit2D hit = new RaycastHit2D();

		float maxRayDis = 10;
		//Debug.DrawRay(myRay.origin, myRay.direction * maxRayDis, Color.cyan);


		hit = Physics2D.Raycast(myRay.origin, dir);

		if (hit && hit.transform.tag == "Stage") {
			float dis = Vector2.Distance(transform.position, hit.point);
			walljumpDir = (hit.point - (Vector2)transform.position);
			//Debug.Log(dis);
			if (dis < wallJumpRange) {
				//Debug.Log("ON WALL");
				onWall = true;
				vel.y = 0.5f * vel.y;
				spinning = false;
			} else {
				//onWall = false;
			}
		}


	}

	
	void updateBulletUI() {

//		for (int i = 0; i < amountOfBullets; i++) {
//			bulletAmount.text += "• ";
//		}

		//bulletAmount.text = new string ('•', amountOfBullets);


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
			groundedCounter ++;
			if (!prevGrounded) {
				scaleSpd = -.1f;
			}

			fastfall = false;

			// if (down && !crouching) {
			// 	transform.localScale = new Vector3(transform.localScale.x, crouchScale, transform.localScale.z);
			// 	transform.position = new Vector3(transform.position.x, transform.position.y - .254f, transform.position.z);
			// 	crouching = true;
			// 	grounded = true;
			// } else {
			// 	transform.localScale = defaultScale;
			// 	crouching = false;
			// }
        } else {
			groundedCounter = 0;
		}
    }



}
