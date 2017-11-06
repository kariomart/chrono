using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour {
    public float spd;
    public GameObject flash;
    int flashTimer;
    BoxCollider2D box;
	// Use this for initialization
	void Start () {
        flash.transform.parent = null;
        box = GetComponent<BoxCollider2D>();
	}
	
	// Update is called once per frame
	void FixedUpdate () {
        flashTimer++;

        if (flashTimer > 1.5f) {
            Destroy(flash);
        }

        transform.position = new Vector3(transform.position.x, transform.position.y + spd, 0);
        /*if (Physics2D.OverlapArea(transform.TransformPoint(box.bounds.max), transform.TransformPoint(box.bounds.min), LayerMask.GetMask("Platform"))) {
            Destroy(gameObject);
            if(flash != null) {
                Destroy(flash);
            }
        }*/
	}

	void OnTriggerEnter2D(Collider2D coll) {

		Debug.Log ("hit! " + coll.gameObject.name);

		if (coll.gameObject.tag == "Player") {

			coll.GetComponent<PlayerMovementController>().health -= 1;

		}

		Destroy (this.gameObject);	

	}
}
