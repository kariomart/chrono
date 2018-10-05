using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour {

	public Image levelImage;

	public Sprite level1; 

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void onClick(string s) {

		UnityEngine.SceneManagement.SceneManager.LoadScene(s);

	}

	public void onSelect(int x) {

		if (x == 0) {
			levelImage.sprite = level1;
		}
	}
}
