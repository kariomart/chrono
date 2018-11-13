using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetResolutions : MonoBehaviour {

	Resolution[] resolutions;
	public Dropdown resolutionDropdown;
	int resValue;
	bool isFullscreen;

	public Sprite openBox;
	public Sprite closedBox;
	public Image fs;

	// Use this for initialization
	void Start () {

		resolutions = Screen.resolutions;
		resolutionDropdown.ClearOptions();

		isFullscreen = Screen.fullScreen;

		if (isFullscreen) {
			fs.sprite = closedBox;
		} else {
			fs.sprite = openBox;
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

		resolutionDropdown.AddOptions(options); 
		resolutionDropdown.value = currentResolutionIndex;
		resolutionDropdown.RefreshShownValue();
		
	}

	public void SetResolution(int resolutionIndex) {

		Resolution resolution = resolutions[resolutionIndex];
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);

	}

	void OnEnable() {
		resolutionDropdown.Select(); 
	}

	public void ChangeResolution() {
		resolutionDropdown.value = resValue % resolutions.Length;
		resValue ++;
		SetResolution(resValue % resolutions.Length);
	}

	public void toggleFullscreen() {
		if (isFullscreen) {
			isFullscreen = false;
			fs.sprite = closedBox;
		} else {
			isFullscreen = true;
			fs.sprite = openBox;
		}

		Screen.fullScreen = isFullscreen;
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
