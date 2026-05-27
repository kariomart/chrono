using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;

public class TimeManager : MonoBehaviour {

    public PlayerMovementController player1;
    public PlayerMovementController player2;
    public AudioSource music;
    public AudioMixer musicMix;
    public PostProcessingProfile FX;
    public PostProcessingProfile DoubleFX;

    public bool gameOverSlow;
    public int slowCounter;
    public Vector2 pos;

    public float globalTimescale;
    public int timeValue;

    public AudioClip slowSound;

    int lowpassMin = 500;
    int lowpassMax = 22000;

    public ScanlinesEffect scanlines;

    bool slowing;

    void Start() {
        SceneManager.sceneLoaded += LevelLoaded;
        LevelLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy() {
        SceneManager.sceneLoaded -= LevelLoaded;
    }

    void LevelLoaded(Scene scene, LoadSceneMode bop) {
        if (scene.name == "_FINAL_menu") return;

        BulletManager.me.spawnList.Clear();
        GameMaster.me.findUI();
        GameMaster.me.updateUI();
        GameMaster.me.findSpawnPoints();

        // player1/player2 are set here for local play.
        // In online play, PlayerMovementController.OnNetworkSpawn() overrides them
        // after the NetworkObjects are fully spawned.
        player1 = GameMaster.me.player1;
        player2 = GameMaster.me.player2;

        scanlines = Camera.main.GetComponent<ScanlinesEffect>();
        globalTimescale = 1f;

        GameMaster.me.StartCoroutine(GameMaster.me.Countdown(3));

        if (!musicMix && music != null) {
            musicMix = music.outputAudioMixerGroup.audioMixer;
        }
    }

    void FixedUpdate() {
        if (music == null) {
            GameObject audioObj = GameObject.FindGameObjectWithTag("AudioController");
            if (audioObj != null) music = audioObj.GetComponent<AudioSource>();
        }

        // Lazily resolve player references in online play (they arrive after LevelLoaded fires).
        if (player1 == null) player1 = GameMaster.me?.player1;
        if (player2 == null) player2 = GameMaster.me?.player2;

        if (player1 == null || player2 == null) return;
        if (GameMaster.me.GameIsPaused) return;

        // netSlow is Owner-writable and synced to all clients, so reading player.slow
        // here gives the correct value on every machine without any extra changes.
        bool p1Slow  = player1.slow;
        bool p2Slow  = player2.slow;
        bool p1Speed = player1.speed;
        bool p2Speed = player2.speed;

        if (!p1Slow && !p2Slow && !slowing) NormalTime();
        if (p1Slow || p2Slow)               SlowTime();
        if (p1Speed || p2Speed)             SpeedTime();
        if ((p1Slow && p2Slow) || gameOverSlow) DoubleSlow();
        if ((p1Slow && p2Speed) || (p1Speed && p2Slow)) NormalTime();
        if (p1Speed && p2Speed) SpeedTime();

        if (gameOverSlow) {
            slowCounter++;
            if (slowCounter > 150) { gameOverSlow = false; slowCounter = 0; }
        } else {
            slowCounter = 0;
        }
    }

    void SlowTime() {
        Time.timeScale = 0.25f;
        Time.fixedDeltaTime = Time.timeScale * 1 / 60f;
        music.pitch = 0.75f;
        GameMaster.me.AddSlowFX();
        if (scanlines != null) scanlines.displacementSpeed = 0.038f;
    }

    void DoubleSlow() {
        Time.timeScale = 0.08f;
        Time.fixedDeltaTime = Time.timeScale * 1 / 60f;
        music.pitch = 0.50f;
        GameMaster.me.AddSlowFX();
        if (scanlines != null) scanlines.displacementSpeed = 0.038f;
    }

    void SpeedTime() {
        Time.timeScale = 1.5f;
        Time.fixedDeltaTime = Time.timeScale * 1 / 60f;
        music.pitch = 1.25f;
    }

    void NormalTime() {
        Time.timeScale = 1f;
        Time.fixedDeltaTime = Time.timeScale * 1 / 60f;
        music.pitch = 1f;
        if (musicMix != null) musicMix.SetFloat("lowpassFreq", 22000);
        GameMaster.me.RemoveSlowFX();
        if (scanlines != null) scanlines.displacementSpeed = 0.525f;
    }

    public void TimeLord(bool slowLast, float time) {
        StopAllCoroutines();
        if (slowLast) StartCoroutine(SpeedTimeForDuration(time));
        else          StartCoroutine(SlowTimeForDuration(time));
    }

    public IEnumerator SlowTimeForDuration(float time) {
        SlowTime();
        slowing = true;
        yield return new WaitForSecondsRealtime(time);
        NormalTime();
        slowing = false;
    }

    public IEnumerator SpeedTimeForDuration(float time) {
        SpeedTime();
        slowing = true;
        yield return new WaitForSecondsRealtime(time);
        NormalTime();
        slowing = false;
    }
}
