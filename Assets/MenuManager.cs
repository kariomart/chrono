using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{

    public GameObject selection;
    public Button startButton;

    // Start is called before the first frame update
    void Start()
    {
        startButton.Select();
    }

    // Update is called once per frame
    void Update()
    {

        
    }

    public void startPressed() {

        Debug.Log("settings enabled");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(Random.Range(1, 6));
    }

    public void settingsEnabled() {

        Debug.Log("settings enabled");

    }

    public void controlsEnabled() {
        Debug.Log("controls enabled");

    }

    public void creditsEnabled() {
        Debug.Log("credits enabled");

    }

    void moveSelection() {


    }
}
