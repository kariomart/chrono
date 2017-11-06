using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {
    Vector2 vel;
    bool jumpFlag;
    bool grounded;
    public Transform groundPt1;
    public Transform groundPt2;
    public float jumpSpd;
    public float gravity;
    public float unjumpBonusGrav;
    Rigidbody2D rb;
    int jmpTimer;
    public float runMaxSpd;
    public float airMaxSpd;
    public float runAccel;
    public float airAccel;
    bool spinning;
    public Transform sprite;
    public float spinSpd;
    public float spinDir;
    Vector3 defSprScale;
    public GameObject bullet;
    public int shotCoolDown;
    int shotTimer;
    bool safety;
    public float bulletYSpd;
	public float slowdownModifier = 0.05f;
	public float slowdownLength = 2f;
	public bool timeSlowed;
    BoxCollider2D box;
    Vector2[] debugPts;
    
    
	// Use this for initialization
	void Start () {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        defSprScale = sprite.localScale;
        debugPts = new Vector2[2];
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetKeyDown (KeyCode.LeftShift)) {
			jumpFlag = true;
		}

		if (Input.GetKey (KeyCode.Z)) {

			SlowDownTime ();
			
		} else {

			Time.timeScale = 1;
			Time.fixedDeltaTime = 1f / 60f;

		}
	}

    private void FixedUpdate() {
        bool right = Input.GetKey(KeyCode.RightArrow);
        bool left = Input.GetKey(KeyCode.LeftArrow);

        SetGrounded();
        if (!grounded && spinning) {
			sprite.eulerAngles = new Vector3(0, 0, sprite.eulerAngles.z + ((spinSpd) * spinDir));
            sprite.localScale = new Vector3(defSprScale.x, defSprScale.y * .75f, 1f);
        } else {
            sprite.eulerAngles = Vector3.zero;
            sprite.localScale = defSprScale;
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
            accel = airAccel ;
            mx = airMaxSpd;
        }
        if (vel.y > 0 && !Input.GetKey(KeyCode.LeftShift)) {
			vel.y -= unjumpBonusGrav * Time.fixedDeltaTime;
        }

        if (right) {
			vel.x += accel * Time.fixedDeltaTime;
        }
        if (left) {
			vel.x -= accel * Time.fixedDeltaTime;
        }

		vel.x = Mathf.Max(Mathf.Min(vel.x, mx), -mx);

        if (!right && !left) {
            vel.x = 0;
        }

        jumpFlag = false;
        shotTimer--;
		rb.MovePosition((Vector2)transform.position + vel * Time.fixedDeltaTime);
    }

	public void SlowDownTime() {

		Time.timeScale = slowdownModifier;
		Time.fixedDeltaTime = Time.timeScale * (1f / 60f); 

	}
		

    void SetGrounded() {
        Vector2 pt1 = transform.TransformPoint(box.offset + new Vector2(box.size.x / 2, -box.size.y / 2));//(box.size / 2));
        Vector2 pt2 = transform.TransformPoint(box.offset - (box.size / 2) + new Vector2(0, 0));
        debugPts[0] = pt1;
        debugPts[1] = pt2;
        grounded = Physics2D.OverlapArea(pt1, pt2, LayerMask.GetMask("Platform")) != null;
        if (grounded) {
            spinning = false;
            vel.y = 0;
            safety = true;
        }
    }

}
