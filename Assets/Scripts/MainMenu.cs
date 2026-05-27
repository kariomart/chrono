using System.Collections;
using UnityEngine;
using Rewired;
using UnityEngine.SceneManagement;
using Kino;
using UnityEngine.Audio;
using TMPro;
using Unity.Netcode;
using Steamworks.Data;

public class MainMenu : MonoBehaviour {

    public Player p1;
    public Player p2;
    public GameObject TV;
    public AnalogGlitchController analogGlitchController;
    AnalogGlitch glitch;
    public int timer;
    public int maxTime;

    public AudioMixer ambienceMix;
    public AudioSource ambience;

    bool gameStarted;

    public GameObject settings;
    bool settingsEnabled;
    public SetResolutions res;
    public GameObject rewiredManager;

    // --- Online UI ---
    // Create these GameObjects in the _FINAL_menu scene and wire them up in the inspector.
    // onlinePanel:    panel with "Host" and "Join" buttons
    // hostWaitPanel:  shown after hosting; contains joinCodeDisplay and a "Waiting..." label
    // joinPanel:      shown when joining; contains joinCodeInput and a confirm button
    // joinCodeDisplay: TMP_Text that shows the 6-char relay code to share
    // joinCodeInput:  TMP_InputField where the joiner types the code
    [Header("Online Lobby UI (optional)")]
    public GameObject onlinePanel;
    public GameObject hostWaitPanel;
    public GameObject joinPanel;
    public TMP_Text joinCodeDisplay;
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;

    static readonly string[] levelNames = {
        "FINAL_level1", "FINAL_level2", "FINAL_level3",
        "FINAL_level4", "FINAL_level5", "FINAL_level6", "FINAL_level7"
    };

    void Start() {
        p1 = ReInput.players.GetPlayer(0);
        p2 = ReInput.players.GetPlayer(1);
        analogGlitchController = Camera.main.GetComponent<AnalogGlitchController>();
        glitch = Camera.main.GetComponent<AnalogGlitch>();
        ambienceMix = ambience.outputAudioMixerGroup.audioMixer;

        if (!GameObject.Find("Rewired Input Manager")) {
            Instantiate(rewiredManager);
        }
    }

    void Update() {

        if ((p1.GetButtonDown("Start") || p2.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Space)) && !gameStarted) {
            if (onlinePanel != null) {
                // Show the online lobby panel instead of jumping straight into a level.
                onlinePanel.SetActive(true);
            } else {
                // No online UI wired up — fall back to local play.
                StartLocalGame();
            }
        }

        if ((p1.GetButtonDown("Select") || p2.GetButtonDown("Select") || Input.GetKeyDown(KeyCode.Escape)) && !gameStarted) {
            if (!settingsEnabled) {
                settings.SetActive(true);
                settingsEnabled = true;
            } else {
                settings.SetActive(false);
                settingsEnabled = false;
            }
        }

        if (p1.GetButtonDown("Restart") || (p2.GetButtonDown("Restart") && settingsEnabled)) {
            res.ChangeResolution();
        }

        if (p1.GetButtonDown("Back") || (p2.GetButtonDown("Back") && settingsEnabled)) {
            res.toggleFullscreen();
        }

        if (gameStarted && timer <= maxTime) {
            glitch.colorDrift += 0.01f;
            glitch.scanLineJitter += 0.01f;
            float d;
            ambienceMix.GetFloat("Distortion", out d);
            ambienceMix.SetFloat("Distortion", d + 0.0035f);
            timer++;
        }
    }

    // --- Local play (original flow) ---

    void StartLocalGame() {
        gameStarted = true;
        analogGlitchController.enabled = false;
        StartCoroutine(LocalStartCoroutine());
    }

    IEnumerator LocalStartCoroutine() {
        yield return new WaitForSeconds(2);
        string level = levelNames[Random.Range(0, levelNames.Length)];
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);
        while (!asyncLoad.isDone) yield return null;
    }

    // --- Online play (called from UI buttons) ---

    // Wire the "Local Play" button's onClick to this.
    public void OnLocalPlayButton() {
        if (onlinePanel != null) onlinePanel.SetActive(false);
        StartLocalGame();
    }

    // Wire the "Host" button's onClick to this.
    public async void OnHostButton() {
        SetStatus("Creating game...");
        try {
            string lobbyId = await LobbyManager.me.StartHost();
            if (joinCodeDisplay != null) joinCodeDisplay.text = lobbyId;
            if (onlinePanel != null) onlinePanel.SetActive(false);
            if (hostWaitPanel != null) hostWaitPanel.SetActive(true);
            SetStatus($"Lobby ID: {lobbyId}\nInvite via Steam or share ID.");

            // Load a level as soon as the second player connects.
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        } catch (System.Exception e) {
            SetStatus($"Error: {e.Message}");
            Debug.LogError($"[MainMenu] Host failed: {e}");
        }
    }

    // Wire the "Join" button's onClick to this.
    public async void OnJoinButton() {
        string code = joinCodeInput != null ? joinCodeInput.text : "";
        if (string.IsNullOrWhiteSpace(code)) {
            SetStatus("Enter a join code first.");
            return;
        }
        SetStatus("Joining...");
        try {
            if (!ulong.TryParse(code, out ulong lobbyIdVal)) {
                SetStatus("Invalid lobby ID.");
                return;
            }
            await LobbyManager.me.JoinLobby((SteamId)lobbyIdVal);
            SetStatus("Connected! Loading level...");
            // Host will trigger a NetworkSceneManager scene load; client just waits.
        } catch (System.Exception e) {
            SetStatus($"Error: {e.Message}");
            Debug.LogError($"[MainMenu] Join failed: {e}");
        }
    }

    // Wire the "Show Join Panel" button's onClick to this.
    public void OnShowJoinPanel() {
        if (onlinePanel != null) onlinePanel.SetActive(false);
        if (joinPanel != null) joinPanel.SetActive(true);
    }

    // --- Private helpers ---

    void OnClientConnected(ulong clientId) {
        // Ignore the host's own connection event.
        if (clientId == NetworkManager.Singleton.LocalClientId) return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;

        string level = levelNames[Random.Range(0, levelNames.Length)];
        Debug.Log($"[MainMenu] Client {clientId} connected. Loading {level}.");
        NetworkManager.Singleton.SceneManager.LoadScene(level, LoadSceneMode.Single);
    }

    void SetStatus(string msg) {
        if (statusText != null) statusText.text = msg;
        Debug.Log($"[MainMenu] {msg}");
    }
}
