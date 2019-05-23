using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Rewired;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Kino;

public class MenuManager : MonoBehaviour
{

    private Player player;

    public GameObject selection;
    public Button startButton;
    public Button lastSelectionButton;

    public GameObject startMenu;
    public GameObject controlObject;
    public GameObject settingsObject;
    public GameObject creditsObject;

    bool settingsOpen;
    bool controlsOpen;
    bool creditsOpen;

    AudioSource audioSource;
    public AudioClip selectionSound;
    public AudioClip pressedSound;
    public AudioClip backSound;

    public Button settingsDefaultSelection;

    AnalogGlitch analogGlitch;



    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        analogGlitch = Camera.main.GetComponent<AnalogGlitch>();
        startButton.Select();
        player = ReInput.players.GetPlayer(0);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (player.GetButtonDown("UICancel")) {
            if (settingsOpen) {
                settingsDisabled();
            }

            if (controlsOpen) {
                controlsDisabled();
            }

            if (creditsOpen) {
                creditsDisabled();
            }

        }

        if (Input.GetKeyDown(KeyCode.S)) {
            Debug.Log("screenshot taken");
            ScreenCapture.CaptureScreenshot("chrono_screenshot.png1", 8);
        }
        
    }

    public void startPressed() {

        Debug.Log("settings enabled");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Random.Range(1, 6));

    }

    public void settingsEnabled() {
        Debug.Log("settings enabled");
        settingsOpen = true;
        startMenu.SetActive(false);
        settingsObject.SetActive(true);
        lastSelectionButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        settingsDefaultSelection.Select();
        StartCoroutine(screenStatic());
    }

    public void settingsDisabled() {
        settingsOpen = false;
        settingsObject.SetActive(false);
        startMenu.SetActive(true);  
        lastSelectionButton.Select();
        StartCoroutine(screenStatic());
    }

    public void controlsEnabled() {
        Debug.Log("controls enabled");
        controlsOpen = true;
        startMenu.SetActive(false);
        controlObject.SetActive(true);
        lastSelectionButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        StartCoroutine(screenStatic());
    }

    public void controlsDisabled() {
        controlsOpen = false;
        controlObject.SetActive(false);
        startMenu.SetActive(true);
        lastSelectionButton.Select();
        StartCoroutine(screenStatic());
    }

    public void creditsEnabled() {
        Debug.Log("credits enabled");
        creditsOpen = true;
        startMenu.SetActive(false);
        creditsObject.SetActive(true);
        lastSelectionButton = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        StartCoroutine(screenStatic());
    }

     public void creditsDisabled() {
        creditsOpen = false;
        creditsObject.SetActive(false);
        startMenu.SetActive(true);
        lastSelectionButton.Select();
        StartCoroutine(screenStatic());
    }

    public IEnumerator screenStatic() {

        audioSource.PlayOneShot(pressedSound);
        analogGlitch.scanLineJitter=1f;
        analogGlitch.colorDrift=.5f;

        for (int i = 0; i < 10; i++)
        {
            analogGlitch.scanLineJitter-=.1f;
            analogGlitch.colorDrift-=.1f;
            yield return new WaitForSeconds(.08f);   
        }
    }

    public void selectionChangeSound() {
        audioSource.PlayOneShot(selectionSound);
    }
}
