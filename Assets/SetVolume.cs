using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetVolume : MonoBehaviour {

	public AudioMixer musicMix;
	public Slider slider;
	

	// Use this for initialization
	void Start () {

	//	slider = GetComponent<Slider>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetLevel(float val) {

		musicMix.SetFloat("Volume", Mathf.Log10(val) * 20f);
		
	}

	void OnEnable() {

		slider.Select();

	}
}
