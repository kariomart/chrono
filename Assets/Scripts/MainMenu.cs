using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Rewired;
using UnityEngine.SceneManagement;
using Kino;
using UnityEngine.Audio;
using TMPro;
using Unity.Netcode;
using Steamworks;

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

    [Header("Online Lobby UI (optional)")]
    public GameObject onlinePanel;
    public GameObject hostWaitPanel;
    public GameObject joinPanel;
    public GameObject joinWaitPanel;
    public TMP_Text joinCodeDisplay;
    public TMP_InputField joinCodeInput;
    public TMP_Text statusText;

    [Header("Lobby member display")]
    public TMP_Text lobbyP1Text;
    public TMP_Text lobbyP2Text;
    public TMP_Text joinLobbyP1Text;
    public TMP_Text joinLobbyP2Text;
    public Button startGameButton;

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

        Debug.Log($"[MainMenu] Start — inspector field audit:");
        Debug.Log($"  onlinePanel    = {(onlinePanel    != null ? onlinePanel.name    : "NULL")}");
        Debug.Log($"  hostWaitPanel  = {(hostWaitPanel  != null ? hostWaitPanel.name  : "NULL")}");
        Debug.Log($"  joinPanel      = {(joinPanel      != null ? joinPanel.name      : "NULL")}");
        Debug.Log($"  joinWaitPanel  = {(joinWaitPanel  != null ? joinWaitPanel.name  : "NULL")}");
        Debug.Log($"  joinCodeDisplay= {(joinCodeDisplay!= null ? "OK"                : "NULL")}");
        Debug.Log($"  joinCodeInput  = {(joinCodeInput  != null ? "OK"                : "NULL")}");
        Debug.Log($"  statusText     = {(statusText     != null ? "OK"                : "NULL")}");
        Debug.Log($"  lobbyP1Text    = {(lobbyP1Text    != null ? "OK"                : "NULL")}");
        Debug.Log($"  lobbyP2Text    = {(lobbyP2Text    != null ? "OK"                : "NULL")}");
        Debug.Log($"  joinLobbyP1Text= {(joinLobbyP1Text!= null ? "OK"               : "NULL")}");
        Debug.Log($"  joinLobbyP2Text= {(joinLobbyP2Text!= null ? "OK"               : "NULL")}");
        Debug.Log($"  startGameButton= {(startGameButton!= null ? "OK"               : "NULL")}");

    }

    void OnEnable() {
        if (LobbyManager.me != null) {
            LobbyManager.me.OnLobbyUpdated -= RefreshLobbyDisplay; // prevent double-subscribe
            LobbyManager.me.OnLobbyUpdated += RefreshLobbyDisplay;
            Debug.Log("[MainMenu] OnEnable — subscribed to LobbyManager.OnLobbyUpdated.");
        } else {
            Debug.LogWarning("[MainMenu] OnEnable — LobbyManager.me is null, could not subscribe. RefreshLobbyDisplay will not fire automatically.");
        }
    }

    void OnDisable() {
        if (LobbyManager.me != null) {
            LobbyManager.me.OnLobbyUpdated -= RefreshLobbyDisplay;
            Debug.Log("[MainMenu] OnDisable — unsubscribed from LobbyManager.OnLobbyUpdated.");
        }
    }

    void OnDestroy() {
        if (LobbyManager.me != null)
            LobbyManager.me.OnLobbyUpdated -= RefreshLobbyDisplay;
    }

    void Update() {

        if ((p1.GetButtonDown("Start") || p2.GetButtonDown("Start") || Input.GetKeyDown(KeyCode.Space)) && !gameStarted) {
            if (onlinePanel != null) {
                onlinePanel.SetActive(true);
            } else {
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

    // --- Local play ---

    void StartLocalGame() {
        gameStarted = true;
        if (analogGlitchController != null) analogGlitchController.enabled = false;
        StartCoroutine(LocalStartCoroutine());
    }

    IEnumerator LocalStartCoroutine() {
        yield return new WaitForSeconds(2);
        string level = levelNames[Random.Range(0, levelNames.Length)];
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(level);
        while (!asyncLoad.isDone) yield return null;
    }

    // --- Online play ---

    public void OnLocalPlayButton() {
        Debug.Log("[MainMenu] OnLocalPlayButton called.");
        if (onlinePanel != null) onlinePanel.SetActive(false);
        StartLocalGame();
    }

    public async void OnHostButton() {
        Debug.Log("[MainMenu] OnHostButton called.");
        SetStatus("Creating game...");
        try {
            Debug.Log("[MainMenu] Calling LobbyManager.StartHost...");
            string lobbyId = await LobbyManager.me.StartHost();
            Debug.Log($"[MainMenu] StartHost returned lobbyId={lobbyId ?? "null"}");
            if (lobbyId == null) {
                Debug.LogError("[MainMenu] StartHost returned null — aborting host setup.");
                SetStatus("Failed to create lobby.");
                return;
            }

            Debug.Log($"[MainMenu] Setting up host wait panel. joinCodeDisplay={joinCodeDisplay != null} onlinePanel={onlinePanel != null} hostWaitPanel={hostWaitPanel != null} startGameButton={startGameButton != null}");

            if (joinCodeDisplay != null) {
                joinCodeDisplay.text = lobbyId;
                Debug.Log($"[MainMenu] Lobby ID displayed: {lobbyId}");
            } else {
                Debug.LogWarning("[MainMenu] joinCodeDisplay is NULL — lobby ID not shown.");
            }

            if (onlinePanel != null) {
                onlinePanel.SetActive(false);
                Debug.Log("[MainMenu] onlinePanel hidden.");
            } else {
                Debug.LogWarning("[MainMenu] onlinePanel is NULL — cannot hide it.");
            }

            if (hostWaitPanel != null) {
                hostWaitPanel.SetActive(true);
                Debug.Log("[MainMenu] hostWaitPanel shown.");
            } else {
                Debug.LogWarning("[MainMenu] hostWaitPanel is NULL — host wait UI will not appear. Run CHRONO > Generate Lobby UI.");
            }

            if (startGameButton != null) {
                startGameButton.interactable = false;
                Debug.Log("[MainMenu] startGameButton disabled (waiting for 2 players).");
            } else {
                Debug.LogWarning("[MainMenu] startGameButton is NULL — Start Game button not available.");
            }

            SetStatus("");

            // Re-subscribe here in case OnEnable/Start missed it due to init order.
            if (LobbyManager.me != null) {
                LobbyManager.me.OnLobbyUpdated -= RefreshLobbyDisplay;
                LobbyManager.me.OnLobbyUpdated += RefreshLobbyDisplay;
                Debug.Log("[MainMenu] OnHostButton — (re)subscribed to OnLobbyUpdated.");
            }

            Debug.Log("[MainMenu] Calling RefreshLobbyDisplay after host setup...");
            RefreshLobbyDisplay();
            StartPeriodicRefresh();
        } catch (System.Exception e) {
            SetStatus($"Error: {e.Message}");
            Debug.LogError($"[MainMenu] OnHostButton exception: {e}");
        }
    }

    public async void OnJoinButton() {
        string code = joinCodeInput != null ? joinCodeInput.text : "";
        Debug.Log($"[MainMenu] OnJoinButton called. code='{code}' joinCodeInput={joinCodeInput != null}");
        if (string.IsNullOrWhiteSpace(code)) {
            SetStatus("Enter a join code first.");
            Debug.LogWarning("[MainMenu] Join aborted — empty code.");
            return;
        }
        SetStatus("Joining...");
        try {
            if (!ulong.TryParse(code, out ulong lobbyIdVal)) {
                SetStatus("Invalid lobby ID.");
                Debug.LogError($"[MainMenu] Could not parse '{code}' as ulong lobby ID.");
                return;
            }
            Debug.Log($"[MainMenu] Parsed lobbyId={lobbyIdVal}. Calling LobbyManager.JoinLobby...");
            await LobbyManager.me.JoinLobby((SteamId)lobbyIdVal);

            bool hasLobby = LobbyManager.me.CurrentLobby.HasValue;
            Debug.Log($"[MainMenu] JoinLobby awaited. CurrentLobby.HasValue={hasLobby}");
            if (hasLobby) {
                SetStatus("");
                Debug.Log($"[MainMenu] Join succeeded. Switching panels. joinPanel={joinPanel != null} joinWaitPanel={joinWaitPanel != null}");
                if (joinPanel     != null) { joinPanel    .SetActive(false); Debug.Log("[MainMenu] joinPanel hidden."); }
                else Debug.LogWarning("[MainMenu] joinPanel is NULL.");
                if (joinWaitPanel != null) { joinWaitPanel.SetActive(true);  Debug.Log("[MainMenu] joinWaitPanel shown."); }
                else Debug.LogWarning("[MainMenu] joinWaitPanel is NULL — joiner wait UI will not appear. Run CHRONO > Generate Lobby UI.");
                // Re-subscribe here in case OnEnable/Start missed it due to init order.
                if (LobbyManager.me != null) {
                    LobbyManager.me.OnLobbyUpdated -= RefreshLobbyDisplay;
                    LobbyManager.me.OnLobbyUpdated += RefreshLobbyDisplay;
                    Debug.Log("[MainMenu] OnJoinButton — (re)subscribed to OnLobbyUpdated.");
                }

                Debug.Log("[MainMenu] Calling RefreshLobbyDisplay after join...");
                RefreshLobbyDisplay();
                StartPeriodicRefresh();
            } else {
                SetStatus("Failed to join lobby.");
                Debug.LogError("[MainMenu] CurrentLobby still null after JoinLobby — join may have failed silently.");
            }
        } catch (System.Exception e) {
            SetStatus($"Error: {e.Message}");
            Debug.LogError($"[MainMenu] OnJoinButton exception: {e}");
        }
    }

    public void OnShowJoinPanel() {
        Debug.Log($"[MainMenu] OnShowJoinPanel. onlinePanel={onlinePanel != null} joinPanel={joinPanel != null}");
        if (onlinePanel != null) onlinePanel.SetActive(false);
        if (joinPanel   != null) joinPanel  .SetActive(true);
    }

    public void OnRefreshLobbyButton() {
        Debug.Log("[MainMenu] OnRefreshLobbyButton — manual refresh requested.");
        RefreshLobbyDisplay();
    }

    public void OnBackToOnlinePanel() {
        Debug.Log("[MainMenu] OnBackToOnlinePanel called — disconnecting and returning to online panel.");
        StopPeriodicRefresh();
        LobbyManager.me?.Disconnect();
        if (hostWaitPanel != null) hostWaitPanel.SetActive(false);
        if (joinPanel     != null) joinPanel    .SetActive(false);
        if (joinWaitPanel != null) joinWaitPanel.SetActive(false);
        if (onlinePanel   != null) onlinePanel  .SetActive(true);
        SetStatus("");
    }

    void StartPeriodicRefresh() {
        CancelInvoke(nameof(PeriodicRefresh));
        InvokeRepeating(nameof(PeriodicRefresh), 3f, 3f);
        Debug.Log("[MainMenu] Periodic lobby refresh started (every 3s).");
    }

    void StopPeriodicRefresh() {
        CancelInvoke(nameof(PeriodicRefresh));
        Debug.Log("[MainMenu] Periodic lobby refresh stopped.");
    }

    void PeriodicRefresh() {
        if (LobbyManager.me == null || !LobbyManager.me.CurrentLobby.HasValue) return;
        int members = LobbyManager.me.CurrentLobby.Value.MemberCount;
        Debug.Log($"[MainMenu] PeriodicRefresh — MemberCount={members}");
        RefreshLobbyDisplay();
    }

    public void OnCopyLobbyId() {
        if (joinCodeDisplay != null) {
            GUIUtility.systemCopyBuffer = joinCodeDisplay.text;
            Debug.Log($"[MainMenu] Copied lobby ID to clipboard: {joinCodeDisplay.text}");
        } else {
            Debug.LogWarning("[MainMenu] OnCopyLobbyId — joinCodeDisplay is NULL.");
        }
    }

    public void OnInviteFriends() {
        var lobby = LobbyManager.me?.CurrentLobby;
        Debug.Log($"[MainMenu] OnInviteFriends — CurrentLobby.HasValue={lobby.HasValue}");
        if (lobby.HasValue)
            SteamFriends.OpenGameInviteOverlay(lobby.Value.Id);
    }

    public void OnStartGameButton() {
        Debug.Log($"[MainMenu] OnStartGameButton called. NetworkManager={NetworkManager.Singleton != null} IsHost={NetworkManager.Singleton?.IsHost}");
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsHost) {
            Debug.LogWarning("[MainMenu] OnStartGameButton — not the host, ignoring.");
            return;
        }
        string level = levelNames[Random.Range(0, levelNames.Length)];
        Debug.Log($"[MainMenu] Host loading scene: {level}");
        NetworkManager.Singleton.SceneManager.LoadScene(level, LoadSceneMode.Single);
    }

    // --- Lobby display ---

    void RefreshLobbyDisplay() {
        var lobbyOpt = LobbyManager.me?.CurrentLobby;
        Debug.Log($"[MainMenu] RefreshLobbyDisplay — CurrentLobby.HasValue={lobbyOpt.HasValue}");
        if (!lobbyOpt.HasValue) {
            Debug.LogWarning("[MainMenu] RefreshLobbyDisplay skipped — CurrentLobby is null.");
            return;
        }

        string p1name = "---", p2name = "---";
        int count = 0;
        foreach (var m in lobbyOpt.Value.Members) {
            if      (count == 0) p1name = m.Name;
            else if (count == 1) p2name = m.Name;
            count++;
        }
        Debug.Log($"[MainMenu] Lobby members: count={count} p1='{p1name}' p2='{p2name}'");

        if (lobbyP1Text     != null) { lobbyP1Text    .text = $"P1  {p1name}"; }
        else Debug.LogWarning("[MainMenu] lobbyP1Text is NULL — host P1 slot not updated.");

        if (lobbyP2Text     != null) { lobbyP2Text    .text = $"P2  {p2name}"; }
        else Debug.LogWarning("[MainMenu] lobbyP2Text is NULL — host P2 slot not updated.");

        if (joinLobbyP1Text != null) { joinLobbyP1Text.text = $"P1  {p1name}"; }
        else Debug.LogWarning("[MainMenu] joinLobbyP1Text is NULL — joiner P1 slot not updated.");

        if (joinLobbyP2Text != null) { joinLobbyP2Text.text = $"P2  {p2name}"; }
        else Debug.LogWarning("[MainMenu] joinLobbyP2Text is NULL — joiner P2 slot not updated.");

        bool canStart = count >= 2;
        if (startGameButton != null) {
            startGameButton.interactable = canStart;
            Debug.Log($"[MainMenu] startGameButton.interactable set to {canStart} (need 2 players, have {count}).");
        } else {
            Debug.LogWarning("[MainMenu] startGameButton is NULL — cannot enable/disable Start Game.");
        }
    }

    void SetStatus(string msg) {
        if (statusText != null) statusText.text = msg;
        if (!string.IsNullOrEmpty(msg)) Debug.Log($"[MainMenu] Status: {msg}");
    }
}
