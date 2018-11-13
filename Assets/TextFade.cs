using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextFade : MonoBehaviour {

	SpriteRenderer sprite;
	public TextMeshProUGUI text;
	Color c;

	// Use this for initialization
	void Start () {

	//	sprite = GetComponent<SpriteRenderer>();
		text = GetComponent<TextMeshProUGUI>();
		c = text.color;
		
	}
	
	// Update is called once per frame
	void Update () {

		text.color = new Color(c.r, c.g, c.b, Mathf.PingPong(Time.time, 1));
		
	}
}
