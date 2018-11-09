using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TextFade : MonoBehaviour {

	SpriteRenderer sprite;
	Color c;

	// Use this for initialization
	void Start () {

		sprite = GetComponent<SpriteRenderer>();
		c = sprite.color;
		
	}
	
	// Update is called once per frame
	void Update () {

		sprite.color = new Color(c.r, c.g, c.b, Mathf.PingPong(Time.time, 1));
		
	}
}
