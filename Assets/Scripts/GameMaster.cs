using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.PostProcessing;
using Kino;
using Rewired;
using UnityEngine.Audio;
using Unity.Netcode;

public class GameMaster : NetworkBehaviour {

    public static GameMaster me;
    public AudioClip hoverSoundEffect;
    public AudioClip playSoundEffect;
    public AudioClip dropdownSoundEffect;
    public AudioClip countdownSFX;
    public AudioClip countdownStartSFX;
    public int bestOf;
    public int roundsNeeded;
    public int setsNeeded;
    public int redWins;
    public int blueWins;
    public int redSets;
    public int blueSets;
    public string winner;
    public bool matchOver;
    public int amtBullets;
    int amountOfLevels;
    public bool roundOver;

    public TextMesh redScore;
    public TextMesh blueScore;
    public TextMesh redSetsMesh;
    public TextMesh blueSetsMesh;
    public GameObject PauseMenu;
    public GameObject TimerUI;
    TextMeshProUGUI TimerNum;
    public GameObject GameOverOverlay;

    public Sprite filledCircle;
    public Sprite filledSquare;
    public SpriteRenderer[] redScoreCircles  = new SpriteRenderer[7];
    public SpriteRenderer[] blueScoreCircles = new SpriteRenderer[7];
    public SpriteRenderer[] redSetsSquares   = new SpriteRenderer[2];
    public SpriteRenderer[] blueSetsSquares  = new SpriteRenderer[2];

    bool gameLoaded;
    public string scene;

    public GameObject[] spawnPoints;
    public GameObject[] bulletSpawnPoints;
    public GameObject redPlayer;
    public GameObject bluePlayer;

    public TimeManager timeMaster;
    public GameObject player1_prefab;
    public GameObject player2_prefab;

    public PlayerMovementController player1;
    public PlayerMovementController player2;
    PostProcessingProfile retroFX_default;
    public AnalogGlitch glitchFX;

    public float fx_baseCA;
    public float fx_baseVignette;
    public float fx_slowVignette;

    public Player controller1;
    public Player controller2;

    public bool GameIsPaused;
    public bool countingDown;

    public int bulletRecentlyStolenTimer;

    public GameObject managers;
    Resolution[] resolutions;
    int resolutionIndex;
    bool musicPaused;
    public GameObject pauseMenu;

    // Helper: true when running without NetworkManager (local play) OR as the server.
    static bool IsServerOrLocal =>
        NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer;

    // --- NetworkVariables ---
    public NetworkVariable<int>  netRedSets   = new NetworkVariable<int> (0,     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<int>  netBlueSets  = new NetworkVariable<int> (0,     NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> netMatchOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);
    public NetworkVariable<bool> netRoundOver = new NetworkVariable<bool>(false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public override void OnNetworkSpawn() {
        netRedSets.OnValueChanged   += (_, v) => { redSets   = v; updateUI(); };
        netBlueSets.OnValueChanged  += (_, v) => { blueSets  = v; updateUI(); };
        netMatchOver.OnValueChanged += (_, v) => matchOver = v;
        netRoundOver.OnValueChanged += (_, v) => roundOver = v;
    }

    // -----------------------------------------------------------------------

    void Awake() {
        if (me == null) {
            me = this;
        } else {
            Destroy(this.gameObject);
        }
        resolutions = Screen.resolutions;
        Cursor.visible = false;
    }

    public void initializeLevel() {
        roundsNeeded = 7;
        setsNeeded   = 2;
        retroFX_default = Camera.main.GetComponent<PostProcessingBehaviour>().profile;
        glitchFX        = Camera.main.GetComponent<AnalogGlitch>();
        setFXDefaults();

        // Only the server (or local play) spawns players.
        if (IsServerOrLocal) {
            SpawnPlayers();
        }

        controller1 = ReInput.players.GetPlayer(0);
        controller2 = ReInput.players.GetPlayer(1);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.Escape)) {
            UnityEngine.SceneManagement.SceneManager.LoadScene("_FINAL_menu");
        }

        if (controller1 == null || controller2 == null) return;

        if (matchOver && (controller1.GetButtonDown("Start") || controller2.GetButtonDown("Start"))) {
            string[] levels = { "FINAL_level1","FINAL_level2","FINAL_level3",
                                 "FINAL_level4","FINAL_level5","FINAL_level6","FINAL_level7" };
            string next = levels[Random.Range(0, levels.Length)];
            if (IsServerOrLocal) {
                if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening)
                    NetworkManager.Singleton.SceneManager.LoadScene(next, UnityEngine.SceneManagement.LoadSceneMode.Single);
                else
                    UnityEngine.SceneManagement.SceneManager.LoadScene(next);
            }
        }

        if ((controller1.GetButtonDown("Select") || controller2.GetButtonDown("Select")) && GameIsPaused) {
            Destroy(GameMaster.me.managers);
            UnityEngine.SceneManagement.SceneManager.LoadScene("_FINAL_menu");
            timeMaster.music.Stop();
            Time.timeScale = 1f;
        }

        bulletRecentlyStolenTimer++;
    }

    public IEnumerator ReEnablePlayer(GameObject obj, GameObject otherPlayerObj) {
        yield return new WaitForSeconds(.9f);
        Vector2 point = getFarthestSpawnPoint(
            otherPlayerObj != null ? (Vector2)otherPlayerObj.transform.position : Vector2.zero);
        obj.transform.position = point;
        obj.SetActive(true);
    }

    public string pickRandomLevel() {
        int rand = Random.Range(1, amountOfLevels + 1);
        return "FINAL_level" + rand;
    }

    public void addToScore(string player, int amt) {
        if (player == "red")  redWins  += amt;
        if (player == "blue") blueWins += amt;
    }

    public void findUI() {
        timeMaster = GameObject.Find("TimeManager").GetComponent<TimeManager>();

        redSetsSquares  = GameObject.Find("redSets").GetComponentsInChildren<SpriteRenderer>();
        blueSetsSquares = GameObject.Find("blueSets").GetComponentsInChildren<SpriteRenderer>();

        redScoreCircles  = GameObject.Find("redCircles").GetComponentsInChildren<SpriteRenderer>();
        blueScoreCircles = GameObject.Find("blueCircles").GetComponentsInChildren<SpriteRenderer>();

        pauseMenu = GameObject.Find("PauseMenu");

        TimerUI = GameObject.Find("Timer");
        TimerNum = TimerUI.GetComponentInChildren<TextMeshProUGUI>(true);
    }

    public void updateUI() {
        if (player1 != null) fillInScore("red",  player1.health);
        if (player2 != null) fillInScore("blue", player2.health);
        fillInSets("red",  redSets);
        fillInSets("blue", blueSets);
    }

    public void hideUI() {
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("score")) g.SetActive(false);
    }

    public void findSpawnPoints() {
        GameObject spawnPointParent = new GameObject("SpawnPoints");
        spawnPoints = GameObject.FindGameObjectsWithTag("SpawnPoint");
        foreach (GameObject s in spawnPoints) {
            s.transform.parent = spawnPointParent.transform;
            s.GetComponent<SpriteRenderer>().enabled = false;
        }

        GameObject bulletSpawnPointParent = new GameObject("BulletSpawnPoints");
        bulletSpawnPoints = GameObject.FindGameObjectsWithTag("BulletSpawner");
        foreach (GameObject b in bulletSpawnPoints) {
            b.transform.parent = bulletSpawnPointParent.transform;
            b.GetComponent<SpriteRenderer>().enabled = false;
        }
    }

    public Vector2 getFarthestSpawnPoint(Vector2 pos) {
        float maxDis = 0;
        Vector2 point = Vector2.zero;
        foreach (GameObject g in spawnPoints) {
            float dis = Vector2.Distance(pos, g.transform.position);
            if (dis > maxDis) { maxDis = dis; point = g.transform.position; }
        }
        return point;
    }

    public void SpawnPlayers() {
        player1 = Instantiate(player1_prefab, LevelSettings.me.spawn1.position, Quaternion.identity).GetComponent<PlayerMovementController>();
        player2 = Instantiate(player2_prefab, LevelSettings.me.spawn2.position, Quaternion.identity).GetComponent<PlayerMovementController>();

        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer) {
            // Player 1 (red) is owned by the host.
            player1.GetComponent<NetworkObject>().SpawnWithOwnership(NetworkManager.Singleton.LocalClientId);

            // Player 2 (blue) is owned by the first non-host client.
            ulong p2Owner = 0;
            foreach (ulong id in NetworkManager.Singleton.ConnectedClientsIds) {
                if (id != NetworkManager.Singleton.LocalClientId) { p2Owner = id; break; }
            }
            player2.GetComponent<NetworkObject>().SpawnWithOwnership(p2Owner);
        } else {
            // Local play: set bullet counts directly.
            player1.amountOfBullets = 2;
            player2.amountOfBullets = 2;
        }

        // otherPlayer cross-references (set here for local play;
        // online play sets them in PlayerMovementController.OnNetworkSpawn).
        if (!NetworkManager.Singleton?.IsListening ?? true) {
            player1.otherPlayer = player2;
            player2.otherPlayer = player1;
        }
    }

    public IEnumerator rumble(PlayerMovementController p, float strength, float length) {
        if (p == null || p.player == null) yield break;
        p.player.SetVibration(p.playerId, strength);
        yield return new WaitForSeconds(length);
        p.player.SetVibration(p.playerId, 0);
    }

    public IEnumerator Countdown(int seconds) {
        countingDown = true;
        Time.timeScale = 0f;
        GameIsPaused = true;
        enableChildren(pauseMenu.transform, false);
        enableChildren(TimerUI.transform, true);
        AudioSource a = TimerUI.GetComponent<AudioSource>();
        a.volume = 0.55f;

        for (int count = seconds; count > 0; count--) {
            TimerNum.text = "" + count;
            a.PlayOneShot(countdownSFX, 1f);
            yield return new WaitForSecondsRealtime(1);
        }

        a.PlayOneShot(countdownStartSFX, 1f);
        countingDown = false;
        Resume();
    }

    public void ChangeResolution() {
        Resolution resolution = resolutions[resolutionIndex % resolutions.Length];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        resolutionIndex++;
    }

    public void Pause() {
        timeMaster.globalTimescale = Time.timeScale;
        Time.timeScale = 0f;
        GameIsPaused = true;
        timeMaster.music.Pause();
        enableChildren(pauseMenu.transform, true);
    }

    public void toggleMusic() {
        if (timeMaster.music.isPlaying) { timeMaster.music.Pause(); musicPaused = true; }
        else { timeMaster.music.Play(); musicPaused = false; }
    }

    public void Resume() {
        enableChildren(TimerUI.transform, false);
        GameIsPaused = false;
        if (!musicPaused) timeMaster.music.Play();
        Time.timeScale = timeMaster.globalTimescale;
    }

    public void enableChildren(Transform o, bool active) {
        foreach (Transform c in o) c.gameObject.SetActive(active);
    }

    public void enableMatchOver() {
        GameObject g = Instantiate(GameOverOverlay, new Vector3(-2.95f, 2.32f, 0), Quaternion.identity);
        g.GetComponentInChildren<TextMeshPro>().text = "the winner is " + winner + " " + redSets + ":" + blueSets;
    }

    void fillInScore(string player, int health) {
        if (player == "red") {
            for (int i = 0; i < 7; i++) redScoreCircles[i].enabled = i < health;
        }
        if (player == "blue") {
            for (int i = 0; i < 7; i++) blueScoreCircles[i].enabled = i < health;
        }
    }

    void fillInSets(string player, int sets) {
        if (player == "red") {
            for (int i = 0; i < sets && i < redSetsSquares.Length; i++) redSetsSquares[i].sprite = filledSquare;
        }
        if (player == "blue") {
            for (int i = 0; i < sets && i < blueSetsSquares.Length; i++) blueSetsSquares[i].sprite = filledSquare;
        }
    }

    public void resetScores() {
        redSets = 0; blueSets = 0;
        if (IsSpawned && IsServer) { netRedSets.Value = 0; netBlueSets.Value = 0; }
    }

    public void AddSlowFX()    { increaseCA(); increaseVignette(); glitchFX.colorDrift += 0.0005f; }
    public void RemoveSlowFX() { decreaseCA(); decreaseVignette(); glitchFX.colorDrift = 0f; }

    public void addMotionBlur()    { retroFX_default.motionBlur.enabled = true; }
    public void removeMotionBlur() { retroFX_default.motionBlur.enabled = false; }

    public void addColorDrift()    { glitchFX.colorDrift = 1; }
    public void removeColorDrift() { glitchFX.colorDrift = 0; }

    public void increaseVignette() {
        var v = retroFX_default.vignette.settings;
        if (v.intensity < fx_slowVignette) v.intensity += .005f;
        retroFX_default.vignette.settings = v;
    }

    public void decreaseVignette() {
        var v = retroFX_default.vignette.settings;
        if (v.intensity > fx_baseVignette) v.intensity -= .01f;
        retroFX_default.vignette.settings = v;
    }

    public void increaseCA() {
        var ca = retroFX_default.chromaticAberration.settings;
        ca.intensity += 0.005f;
        retroFX_default.chromaticAberration.settings = ca;
    }

    public void decreaseCA() {
        var ca = retroFX_default.chromaticAberration.settings;
        if (ca.intensity > fx_baseCA) ca.intensity -= 0.01f;
        retroFX_default.chromaticAberration.settings = ca;
    }

    public void setFXDefaults() {
        var ca = retroFX_default.chromaticAberration.settings;
        ca.intensity = fx_baseCA;
        retroFX_default.chromaticAberration.settings = ca;

        var v = retroFX_default.vignette.settings;
        v.intensity = fx_baseVignette;
        retroFX_default.vignette.settings = v;
    }

    public void SpawnParticle(ParticleSystem fx, Vector2 pos) {
        Instantiate(fx.gameObject, pos, Quaternion.identity);
    }

    public void SpawnParticle(ParticleSystem fx, Vector2 pos, Color c) {
        ParticleSystem p = Instantiate(fx.gameObject, pos, Quaternion.identity).GetComponent<ParticleSystem>();
        var main = p.main;
        main.startColor = c;
    }

    public void SpawnParticle(ParticleSystem fx, Vector2 pos, Color c1, Color c2) {
        ParticleSystem p = Instantiate(fx.gameObject, pos, Quaternion.identity).GetComponent<ParticleSystem>();
        ParticleSystem.MinMaxGradient gradient = new Gradient();
        GradientColorKey[] cK = new GradientColorKey[2];
        GradientAlphaKey[] aK = new GradientAlphaKey[1];
        cK[0].color = c1;
        cK[1].color = c2;
        cK[1].time  = 1;
        aK[0].alpha = 1f;
        gradient.gradient.SetKeys(cK, aK);
        var main = p.main;
        gradient.mode = ParticleSystemGradientMode.Gradient;
        main.startColor = gradient;
    }

    public int getAmountOfLevels() { return amountOfLevels; }
}
