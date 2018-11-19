using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TextFade : MonoBehaviour {

	SpriteRenderer sprite;
	public TextMeshProUGUI UItext;
	public TextMeshPro text;
	Color c;

	// Use this for initialization
	void Start () {

	//	sprite = GetComponent<SpriteRenderer>();
		UItext = GetComponent<TextMeshProUGUI>();
		text = GetComponent<TextMeshPro>();

		if (UItext) {
			c = UItext.color;
		} 

		if(text) {
			c = text.color;
		}
		
	}
	
	// Update is called once per frame
	void Update () {


		if (UItext) {
			UItext.color = new Color(c.r, c.g, c.b, Mathf.PingPong(Time.time, 1));
		} 

		if (text) {
			text.color = new Color(c.r, c.g, c.b, Mathf.PingPong(Time.time, 1));
		} 
		
	}
}
