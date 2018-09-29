using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using InControl;

public class PlayerMovementController : MonoBehaviour {

	public TimeManager fatherTime;
	public PlayerMovementController otherPlayer;

	public int playerId;
	InputDevice player1;
	Rigidbody2D rb;
	BoxCollider2D box;
	CircleCollider2D almostDeadCircle;
	public Transform sprite;
	public TextMesh ammoText;

	Vector3 defSprScale;
	Vector2 debugPts;
	Vector2 defaultScale;
	float scaleSpd;
	public Color playerColor;
	public string colorName;

	public GameObject reticle;
	public GameObject bullet;
	public GameObject manaBar;
	public GameObject pivot;
	public GameObject letterbox;
	public GameObject camera;

	public Vector2 dir;
	public Vector2 vel;
	Vector2 prevVel;
	Vector2 prevDir = new Vector2(0f, 1f);
	Vector2 wallDir;
	Vector2 wallPos;
	public Transform shootPt;
	public float wallJumpRange;
	public float spinSpd;
	public float spinDir;
	public int face;

	public float runAccel;
	public float runMaxSpeed;
	public float groundDrag;
	public float airAccel;
	public float airMaxSpeed;
	public float jumpSpd;
	public float gravity;
	public float bonusGravity;
	public float jumpChargeTimer;
	public float jumpChargeMax;

	public bool right;
	public bool left;
	public bool down;
	public bool slow;
	public bool speed;
	public bool grounded;
	public bool onWall;
	bool spinning;
	bool fastfall;
	bool jumpFlag;
	public bool almostdead;
	public bool gameOver;


	public int health;
	public int amountOfBullets;
	int shotTimer;
	public float bulletTimer;
	public float bulletCooldown; 
	public float mana; 
	public float maxMana;
	public float timeManaDrain;
	public float manaGain;
	public float kick;
	bool onWallRight;
	bool onWallLeft;
	public float wallFriction;

	public int invulnCounter;
	public int invulnMaxFrames;
	public bool invuln;

	public AudioClip shootSound;
	public AudioClip whoosh;
	public AudioClip slowSound;
	public AudioClip speedSound;
	public int score;

	public GameObject hitParticle;


	void Start () {

		almostdead = true;
		rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
		almostDeadCircle = GetComponentInChildren<CircleCollider2D>();
        defSprScale = sprite.localScale;
		defaultScale = pivot.transform.localScale;
		ammoText = GetComponentInChildren<TextMesh>();
		updateUI();
		camera = Camera.main.gameObject;
//		Debug.Log(InputManager.Devices);
		player1 = InputManager.Devices[playerId];

		foreach (GameObject g in GameObject.FindGameObjectsWithTag ("Player")) {
			PlayerMovementController p = g.GetComponent<PlayerMovementController>();
//			Debug.Log(p.playerId);
			if (p.playerId != this.playerId) {
				otherPlayer = p;
			}
		}

		PlayerTuning tuning = Resources.Load<PlayerTuning>("MyTune");

		runAccel = tuning.runAccel;
		runMaxSpeed = tuning.runMaxSpeed;
		groundDrag = tuning.groundDrag;
		airAccel = tuning.airAccel;
		airMaxSpeed = tuning.airMaxSpeed;
		jumpSpd = tuning.jumpSpd;
		gravity = tuning.gravity;
		bonusGravity = tuning.bonusGravity;
		jumpChargeTimer = tuning.jumpChargeTimer;
		jumpChargeMax = tuning.jumpChargeMax;
		kick = tuning.kick;
		wallFriction = tuning.wallFriction;
		maxMana = tuning.maxMana;
		timeManaDrain = tuning.timeManaDrain;
		manaGain = tuning.manaGain;
		bulletCooldown = tuning.bulletCooldown;
		invulnMaxFrames = tuning.invulnMaxFrames;
		health = tuning.health;

	}
	
	void Update () {

	
		right = player1.LeftStickRight && player1.LeftStickRight.Value > .5f;
		left = player1.LeftStickLeft && player1.LeftStickLeft.Value > .5f;
		updateUI();

		dir = new Vector2(player1.LeftStickX, player1.LeftStickY).normalized;
		bulletTimer += Time.deltaTime;

		
		if (dir != Vector2.zero) {
			prevDir = dir;
		}

		if (player1.Action1.IsPressed && jumpChargeTimer < jumpChargeMax) {
			jumpChargeTimer ++;
		}

		if (player1.Action1.WasPressed) {
			jumpFlag = true;
			//Instantiate(jumpEffect)
		}

		if (down && vel.y < 0 && !fastfall) {
			fastfall = true;
		}

		if (player1.Action3.WasPressed && canShoot()) {
			shootBullet();
		}

		if (player1.RightTrigger.Value > 0 && (canSlowTime() || (slow && mana > 0))) {
			if(!slow && !otherPlayer.slow) {
			//	SoundController.me.PlaySound(slowSound, .3f);
			}
			slow = true;
			mana -= timeManaDrain;

			//otherPlayer.mana += timeManaDrain;
		} else {
			if (slow) {
				//SoundController.me.PlaySound(speedSound, .3f);
			}
			slow = false;

			if (mana < maxMana) {
				mana += manaGain;
			}
		}

		// if ((player1.Action2.WasPressed && (gameOver || otherPlayer.gameOver) && !GameMaster.me.matchOver) || Input.GetKeyDown(KeyCode.R)) {
		// 	Time.timeScale = 1f;
		// 	Application.LoadLevel(Application.loadedLevel);
		// }

		if ((player1.Action2.WasPressed && (gameOver || otherPlayer.gameOver) && !GameMaster.me.matchOver)) {
			Time.timeScale = 1f;
			UnityEngine.SceneManagement.SceneManager.LoadScene("level_select");

		}


//		manaBar.transform.localScale = new Vector2 (mana, manaBar.transform.localScale.y);

		if (almostdead) {
			almostDeadCircle.enabled = true;
		}

		if (health <= 0 && !gameOver) {

			gameOver = true;
			Camera.main.GetComponent<CamControl>().enabled = false;
			otherPlayer.health += 10;
			GameMaster.me.redWins = 0;
			GameMaster.me.blueWins = 0;
			GameMaster.me.updateUI();
			Instantiate (letterbox, new Vector2(transform.position.x, transform.position.y - .5f), Quaternion.identity);
			CamGameOver cameraController = camera.GetComponent<CamGameOver> ();
			camera.GetComponent<Screenshake> ().enabled = false;
			camera.transform.position = new Vector3 (transform.position.x, transform.position.y, -10);
			cameraController.playerLost = this.GetComponent<PlayerMovementController> ();
			cameraController.textColor = playerColor;
			cameraController.enabled = true;
			//Destroy (this.gameObject, 1.2f);
		}

		if (mana < 0) {
			mana = 0;
		}

		if (invuln && invulnCounter < invulnMaxFrames) {
			invulnCounter ++;
		} else {
			invuln = false;
		}

	}

	void FixedUpdate() {

		setGrounded();
		wallCast();

		float desYScale = defaultScale.y + (Mathf.Abs(vel.y) * 0.02f);
		scaleSpd += ((desYScale - pivot.transform.localScale.y) * 3f);
		scaleSpd *= 5.4f * Time.fixedDeltaTime;


		float accel = runAccel;
		float mx = runMaxSpeed; //* (1f - (jumpChargeTimer / jumpChargeMax));

		if (!spinning) {
			pivot.transform.localScale = new Vector2 (defaultScale.x + (defaultScale.y - pivot.transform.localScale.y), pivot.transform.localScale.y + scaleSpd);
		}

		if (!grounded && spinning) {
			sprite.eulerAngles = new Vector3(0, 0, sprite.eulerAngles.z + ((spinSpd) * Time.fixedDeltaTime * spinDir));
			pivot.transform.localScale = defaultScale;
		} else {
			sprite.eulerAngles = Vector3.zero;
		}

		if (jumpFlag && grounded) {
			if (left || right) {
				spinning = true;
				transform.localScale = defaultScale;

				if (left) spinDir = 1;
				if (right) spinDir = -1;
			}
			vel.y = jumpSpd; //* (jumpChargeTimer / jumpChargeMax);
			jumpChargeTimer = 0;

		} 
		
		if (onWall && !grounded) {
			//vel = Vector2.zero;
			spinning = false;
			vel.y *= wallFriction;
			if (onWallLeft)
				vel.x = Mathf.Max(vel.x, 0);
			if (onWallRight)
				vel.x = Mathf.Min(vel.x, 0);
			if (jumpFlag) {
				//vel.x = -wallDir.x * 5;
				//vel.y = jumpSpd;
				vel.x = onWallLeft ? 15f : -15f;//new Vector2((-wallDir.x * 10) + dir.x, jumpSpd);
				vel.y = jumpSpd;
				onWall = false;
			}

		}

		if (!grounded) {
			vel.y -= gravity * Time.fixedDeltaTime;
			accel = airAccel;
			//mx = airMaxSpeed;
		}

		if (vel.y > 0 && !player1.Action1.IsPressed) {
			vel.y -= bonusGravity * Time.fixedDeltaTime;
		}

		if (right && (!slow || grounded)) {
			vel.x += accel * Time.fixedDeltaTime;
			face = 1;

        }
        if (left && (!slow || grounded)) {
			vel.x -= accel * Time.fixedDeltaTime;
			face = -1;
        }

		vel.x = Mathf.Clamp(vel.x, -mx, mx);

		if (!left && !right && grounded){
			if (Mathf.Abs(vel.x) < groundDrag * Time.fixedDeltaTime) vel.x = 0;
			else vel.x -= (groundDrag * Mathf.Sign(vel.x)) * Time.fixedDeltaTime;
		}

		jumpFlag = false;
		shotTimer --;

		if (gameOver) {
			vel = Vector2.zero;
		}

		prevVel = vel;
		rb.MovePosition((Vector2)transform.position + vel * Time.fixedDeltaTime);

		 /* if (dir == Vector2.zero) {
			//reticle.transform.position = ((Vector2)(shootPt.position) + prevDir.normalized) * .5f;
		} else { */
			reticle.transform.position = new Vector2 (shootPt.transform.position.x + (dir.x * .5f), shootPt.transform.position.y + (dir.y * .5f)); 
		/*}*/

	}

	void shootBullet() {

		amountOfBullets --;
		bulletTimer = 0;


		if (amountOfBullets > 3) {
			SoundController.me.PlaySound (shootSound, 1f);
		} else {
			//Debug.Log(3 - (amountOfBullets + 1));
			SoundController.me.PlaySound (shootSound, 1f, 3 - (amountOfBullets));
		}
		

		//SoundController.me.PlaySound(shootSound, 1f, maxBullets / (amountOfBullets + 1));
		GameObject tempBullet;

		if (dir.x == 0 && dir.y == 0) {
			tempBullet = Instantiate (bullet, new Vector3(shootPt.position.x + prevDir.x * .5f, shootPt.transform.position.y + prevDir.y * .5f), Quaternion.identity);
			tempBullet.GetComponent<Bullet>().vel = prevDir;
		} else {

			tempBullet = Instantiate (bullet, new Vector3(shootPt.transform.position.x + dir.x * .5f, shootPt.transform.position.y + dir.y * .5f), Quaternion.identity);
			tempBullet.GetComponent<Bullet> ().vel = dir; 
		}
		vel -= dir * kick;
		SoundController.me.PlaySound (whoosh, 0.5f);
		updateUI();


	}

	void setGrounded() {

		Vector2 pt1 = transform.TransformPoint(box.offset + new Vector2(box.size.x / 2, -box.size.y / 2) + new Vector2(-.01f, 0));//(box.size / 2));
        Vector2 pt2 = transform.TransformPoint(box.offset - (box.size / 2) + new Vector2(.01f, 0));
		bool prevGrounded = grounded;
		grounded = Physics2D.OverlapArea(pt1, pt2, LayerMask.GetMask("Pinata")) != null;
        grounded = Physics2D.OverlapArea(pt1, pt2, LayerMask.GetMask("Platform")) != null;
		
        if (grounded) {
            spinning = false;
			if (!prevGrounded) {
//				Debug.Log(prevVel.y);
				scaleSpd = -13f * (Mathf.Abs(prevVel.y) / 30f) * Time.fixedDeltaTime;
			}
            vel.y = 0;


			fastfall = false;
		}

	}
	
	void wallCast() {
		int mask = LayerMask.GetMask("Platform");

		Vector2 top = (Vector2)transform.position + box.offset + (Vector2.up * (box.size.y /2f));
		Vector2 bot = (Vector2)transform.position + box.offset - (Vector2.up * (box.size.y /2f));
		onWallLeft = Physics2D.Raycast(top, Vector2.left, box.size.x * .6f, mask) || Physics2D.Raycast(bot, Vector2.left, box.size.x * .6f, mask);
		onWallRight = Physics2D.Raycast(top, Vector2.right, box.size.x * .6f, mask) || Physics2D.Raycast(bot, Vector2.right, box.size.x * .6f, mask);
		onWall = onWallLeft || onWallRight;
		/*Ray2D myRay = new Ray2D(transform.position, vel);
		RaycastHit2D hit = new RaycastHit2D();
		float dis;
		float maxRayDis = 2;
		Debug.DrawRay(myRay.origin, myRay.direction * maxRayDis, Color.cyan);


		hit = Physics2D.Raycast(myRay.origin, dir);
		dis = Vector2.Distance(transform.position, hit.point);

		if (hit.point != Vector2.zero) {
			wallPos = hit.point;
		}
//		Debug.Log(transform.position + "\n" + hit.point + "\n" + dis);

		if (hit && hit.transform.tag == "Wall") {
			wallDir = (hit.point - (Vector2)transform.position);
			//Debug.Log(dis);
			if (dis < wallJumpRange) {
				onWall = true;
			} 
		} else {

			if (Vector2.Distance(transform.position, wallPos) > wallJumpRange) {
				onWall = false;
			}
		} */
	}

	public void respawn() {

		health --;

		if (health == 1) {
			almostdead = true;
		}

		if (health > 0 && !gameOver) {

			GameMaster.me.addToScore(otherPlayer.colorName, 1);
			GameMaster.me.updateUI();
			Instantiate(hitParticle, transform.position, Quaternion.identity);
			//var main = hitParticle.transform.GetChild(0).GetComponent<ParticleSystem>().main;
			//main.startColor = playerColor;
			otherPlayer.score ++;
			Vector2 point = GameMaster.me.getFarthestSpawnPoint(otherPlayer.transform.position);
			transform.position = point;
			invuln = true;
			amountOfBullets ++;
			invulnCounter = 0;
			pivot.transform.localScale = pivot.transform.localScale * .2f;
			scaleSpd = 1;
			GameMaster.me.StartCoroutine(GameMaster.me.ReEnablePlayer(gameObject));
			gameObject.SetActive(false);
		}

	}

	public void updateUI() {

		ammoText.text = "" + amountOfBullets;

	}


	bool canSlowTime() {

		if (mana - timeManaDrain >= 2) {
			return true;
		} else {
			return false;
		}
	}

	bool canShoot() {
		if (amountOfBullets > 0 && bulletTimer > bulletCooldown) {
			return true;
		} else {
			return false;
		}
	}


	void OnCollisionEnter2D(Collision2D coll) {

		if (coll.contacts.Length > 0) {

			ContactPoint2D pt = coll.contacts[0];
			if (coll.gameObject.layer == LayerMask.NameToLayer("Platform")) {
				vel += pt.normal * Vector2.Dot(-pt.normal, vel);
			}
			if (coll.gameObject.tag == "Player") {
				vel.y = jumpSpd;
			}
			if (coll.gameObject.tag == "Bullet") {
				Bullet bull = coll.gameObject.GetComponent<Bullet> ();
				updateUI();
				//vel = bull.vel * knockbackAmount;
			}
		}

		/*if (coll.gameObject.tag == "Stage" && !grounded) {
			vel *= .3f;
		}*/

		if (coll.gameObject.tag == "Pinata") {
//			Debug.Log(coll.contacts[0].normal);
			Vector2 c = coll.contacts[0].normal;
			vel = new Vector2(c.x, c.y * jumpSpd);
//			Debug.Log("pinata");
			//vel.y *= -.25f;
		}

	}
}