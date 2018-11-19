using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SetVolume : MonoBehaviour {

	public AudioMixer musicMix;
	public AudioSource music;
	public Slider slider;
	

	// Use this for initialization
	void Start () {


		SetLevel(slider.value);

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
