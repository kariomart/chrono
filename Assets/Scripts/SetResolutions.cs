using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using TMPro;

public class SetResolutions : MonoBehaviour {

	public Resolution[] resolutions;
	public Dropdown resolutionDropdown;
	public  TextMeshProUGUI resText;
	public TextMeshProUGUI fullscreenText;
	int resValue;
	bool isFullscreen;
	float musicVol;
	public TextMeshProUGUI musicVal;
	AudioSource menuMusic;
	float sfxVol;
	public AudioMixer sfxMixer;
	public TextMeshProUGUI sfxVal;

	public GameObject managerPrefab;
	PlayerTuning tuning;

	int[] healthVals = {1, 3, 5, 7};
	int healthIndex;
	public TextMeshProUGUI healthVal;



	// Use this for initialization
	void Start () {

		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "_FINAL_menu") { 
			menuMusic = GameObject.Find("MenuMusic").GetComponent<AudioSource>();
			musicVol = menuMusic.volume*100;
			setMusicVolume();
		}
		tuning = Resources.Load<PlayerTuning>("MyTune");
		healthVal.text = "" + tuning.health;
		healthIndex = 2;
		managerPrefab = Resources.Load<GameObject>("MANAGERS");

		resolutions = Screen.resolutions;
		resText = GetComponent<TextMeshProUGUI>();
		resText.text = Screen.currentResolution.width + " x " + Screen.currentResolution.height;
		isFullscreen = Screen.fullScreen;

		if (isFullscreen) {
			fullscreenText.text = "YES";
		} else {
			fullscreenText.text = "NO";
		}

		List<string> options = new List<string>();

		int currentResolutionIndex = 0;
		for (int i = 0; i < resolutions.Length; i++) {

			string option = resolutions[i].width + " x " + resolutions[i].height;	
			options.Add(option);

			if ((resolutions[i].width == Screen.currentResolution.width) && (resolutions[i].height == Screen.currentResolution.height))  {
				currentResolutionIndex = i;
			}
		}

		// resolutionDropdown.AddOptions(options); 
		// resolutionDropdown.value = currentResolutionIndex;
		// resolutionDropdown.RefreshShownValue();
		
	}

	public void SetResolution(int resolutionIndex) {

		//resText.text = resolutions[resolutionIndex].ToString();
		resText.text = Screen.currentResolution.width + " x " + Screen.currentResolution.height;
		Resolution resolution = resolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

	}

	void OnEnable() {
		//resolutionDropdown.Select(); 
	}

	public void ChangeResolution() {
		//resolutionDropdown.value = resValue % resolutions.Length;
		resValue ++;
		SetResolution(resValue % resolutions.Length);
	}

	public void toggleFullscreen() {
		if (isFullscreen) {
			isFullscreen = false;
			fullscreenText.text = "NO";
		} else {
			isFullscreen = true;
			fullscreenText.text = "YES";
		}

		Screen.fullScreen = isFullscreen;
	}

	public void changeMusicVolume() {

		musicVol += 10f;
		musicVol %= 110f;
		setMusicVolume(); 

	}

	void setMusicVolume() {
		musicVal.text = "" + musicVol + "%";
		if (UnityEngine.SceneManagement.SceneManager.GetActiveScene().name == "_FINAL_menu") {
			menuMusic.volume = musicVol/100f;
			managerPrefab.transform.GetChild(3).GetComponent<AudioSource>().volume = musicVol/100;
		} else {
			managerPrefab = GameObject.Find("MANAGERS(Clone)");
			managerPrefab.transform.GetChild(3).GetComponent<AudioSource>().volume = musicVol/100;
		}
	}

	public void changeSFXVolume() {

		sfxVol += 10f;
		sfxVol %= 110f;
		setSFXVolume(); 

	}

	void setSFXVolume() {
		sfxVal.text = "" + sfxVol + "%";
		//AudioMixer sfx = SoundController.me.audSource.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;
		sfxMixer.SetFloat("volume", Mathf.Lerp(-10, 0, sfxVol / 100));
	}

	public void changeHealthVal() {
		healthIndex ++;
		healthIndex %= healthVals.Length;
		healthVal.text = "" + healthVals[healthIndex];
		tuning.health = healthVals[healthIndex];
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
