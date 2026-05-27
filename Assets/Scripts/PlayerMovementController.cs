using System.Collections;
using UnityEngine;
using System.Collections.Generic;
using Rewired;
using Unity.Netcode;

public class PlayerMovementController : NetworkBehaviour {

    public TimeManager fatherTime;
    public PlayerMovementController otherPlayer;

    public int playerId;        // 0 = red (host-owned), 1 = blue (client-owned)
    public Player player;       // Rewired player — always local controller 0 when online
    public Rigidbody2D rb;
    BoxCollider2D box;
    CircleCollider2D almostDeadCircle;
    public Transform sprite;
    public TextMesh ammoText;

    Vector3 defSprScale;
    Vector2 debugPts;
    Vector2 defaultScale;
    float scaleSpd;
    public Color playerColor;
    public string colorName;

    public GameObject reticle;
    Animation reticleScale;
    public GameObject bullet;
    public GameObject manaBar;
    public GameObject pivot;
    public GameObject letterbox;
    public new GameObject camera;

    public Vector2 dir;
    public Vector2 vel;
    Vector2 prevVel;
    Vector2 prevDir = new Vector2(0f, 1f);
    Vector2 wallDir;
    Vector2 wallPos;
    public Transform shootPt;
    public float wallJumpRange;
    public float spinSpd;
    public float spinDir;
    public int face;

    public float runAccel;
    public float runMaxSpeed;
    public float groundDrag;
    public float airAccel;
    public float airMaxSpeed;
    public float jumpSpd;
    public float gravity;
    public float bonusGravity;
    public float jumpChargeTimer;
    public float jumpChargeMax;
    public float maxMapX;
    public float maxMapY;
    public float minMapY;

    public bool right;
    public bool left;
    public bool down;
    public bool slow;
    public bool speed;
    public bool grounded;
    public bool prevGrounded;
    public bool onWall;
    bool spinning;
    bool fastfall;
    int jumpTimer;
    public bool almostdead;
    public bool gameOver;

    public int health;
    public int amountOfBullets;
    int shotTimer;
    public float bulletTimer;
    public float bulletCooldown;
    public float mana;
    public float maxMana;
    public float timeManaDrain;
    public float manaGain;
    public float kick;
    bool onWallRight;
    bool onWallLeft;
    public float wallFriction;

    public int invulnCounter;
    public int invulnMaxFrames;
    public bool invuln;

    public AudioClip shootSound;
    public AudioClip shootSoundSlow;
    public AudioClip whoosh;
    public AudioClip slowSound;
    public AudioClip speedSound;
    public AudioClip jumpSound;
    public AudioClip cantShootSound;

    public int score;

    public GameObject hitParticle;
    public GameObject shootParticle;
    public GameObject muzzleFlash;
    public ParticleSystem jumpParticle;
    public ParticleSystem wallParticle;
    public ParticleSystem landParticle;
    public ParticleSystem moveParticle;

    Screenshake screenshake;

    // --- NetworkVariables ---
    // netHealth: server writes on damage; all clients read to display score UI.
    public NetworkVariable<int> netHealth = new NetworkVariable<int>(
        7, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // netBullets: server writes on shoot/pickup/steal; all clients read for UI.
    public NetworkVariable<int> netBullets = new NetworkVariable<int>(
        2, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    // netSlow: owner writes each frame; server + other client read to drive TimeManager.
    public NetworkVariable<bool> netSlow = new NetworkVariable<bool>(
        false, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    // True when this instance should read local Rewired input.
    // • Not networked at all (local play): IsSpawned == false → always true.
    // • Networked + owner: true.
    // • Networked + remote: false.
    bool ShouldProcessInput => !IsSpawned || IsOwner;

    // -----------------------------------------------------------------------

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        box = GetComponent<BoxCollider2D>();
        almostDeadCircle = GetComponentInChildren<CircleCollider2D>();
        camera = Camera.main.gameObject;
        screenshake = camera.GetComponent<Screenshake>();
        defSprScale = sprite.localScale;
        defaultScale = pivot.transform.localScale;
        ammoText = GetComponentInChildren<TextMesh>();
        reticleScale = reticle.GetComponent<Animation>();

        // Find the other player by tag (works in both local and online).
        // In online play this may fail if the other player isn't spawned yet;
        // OnNetworkSpawn() and the null-guard in Update cover that case.
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
            PlayerMovementController p = g.GetComponent<PlayerMovementController>();
            if (p != null && p.playerId != this.playerId) {
                otherPlayer = p;
            }
        }

        // Local play: use the configured playerId slot.
        // Online:     OnNetworkSpawn() overrides this to always use slot 0.
        player = ReInput.players.GetPlayer(playerId);

        PlayerTuning tuning = Resources.Load<PlayerTuning>("MyTune");
        runAccel = tuning.runAccel;
        runMaxSpeed = tuning.runMaxSpeed;
        groundDrag = tuning.groundDrag;
        airAccel = tuning.airAccel;
        airMaxSpeed = tuning.airMaxSpeed;
        jumpSpd = tuning.jumpSpd;
        gravity = tuning.gravity;
        bonusGravity = tuning.bonusGravity;
        jumpChargeTimer = tuning.jumpChargeTimer;
        jumpChargeMax = tuning.jumpChargeMax;
        kick = tuning.kick;
        wallFriction = tuning.wallFriction;
        maxMana = tuning.maxMana;
        timeManaDrain = tuning.timeManaDrain;
        manaGain = tuning.manaGain;
        bulletCooldown = tuning.bulletCooldown;
        invulnMaxFrames = tuning.invulnMaxFrames;
        health = tuning.health;

        if (GameMaster.me != null) GameMaster.me.updateUI();
    }

    // Called on all clients when this NetworkObject is fully spawned.
    public override void OnNetworkSpawn() {
        if (IsOwner) {
            // Online: always read from the first local controller, regardless of playerId.
            player = ReInput.players.GetPlayer(0);
        }

        // Sync local fields from NetworkVariables on spawn.
        health = netHealth.Value;
        amountOfBullets = netBullets.Value;
        slow = netSlow.Value;

        // Register with GameMaster so it holds the correct references on all machines.
        if (GameMaster.me != null) {
            if (playerId == 0) {
                GameMaster.me.player1 = this;
                if (GameMaster.me.timeMaster != null) GameMaster.me.timeMaster.player1 = this;
            } else {
                GameMaster.me.player2 = this;
                if (GameMaster.me.timeMaster != null) GameMaster.me.timeMaster.player2 = this;
            }
            GameMaster.me.updateUI();
        }

        // Try to resolve otherPlayer reference (may still be null if the other prefab
        // hasn't spawned yet; the null-guard in Update handles the retry).
        foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
            PlayerMovementController p = g.GetComponent<PlayerMovementController>();
            if (p != null && p.playerId != this.playerId) { otherPlayer = p; break; }
        }

        // Disable physics simulation on machines that neither own nor serve this player.
        // Server keeps rb.simulated=true so bullet collision detection works server-side.
        if (IsSpawned && !IsOwner && !IsServer) {
            rb.simulated = false;
        }
    }

    // -----------------------------------------------------------------------

    void Update() {

        // Lazy-resolve otherPlayer if it was null at spawn time.
        if (otherPlayer == null) {
            foreach (GameObject g in GameObject.FindGameObjectsWithTag("Player")) {
                PlayerMovementController p = g.GetComponent<PlayerMovementController>();
                if (p != null && p.playerId != playerId) { otherPlayer = p; break; }
            }
        }

        // --- Remote player: pull state from NetworkVariables and skip input ---
        if (IsSpawned && !IsOwner) {
            slow = netSlow.Value;
            amountOfBullets = netBullets.Value;
            health = netHealth.Value;
            updateUI();
            handleInvuln();
            checkGameOver();
            return;
        }

        // --- Owner / local-play path below ---

        // Sync local fields from NetworkVariables each frame (server is authoritative
        // on health and bullets; reading here keeps the display consistent).
        if (IsSpawned) {
            amountOfBullets = netBullets.Value;
            health = netHealth.Value;
        }

        right = player.GetAxis("MoveHorizontal") > .5f;
        left  = player.GetAxis("MoveHorizontal") < -.5f;

        updateUI();

        dir = new Vector2(player.GetAxis("MoveHorizontal"), player.GetAxis("MoveVertical")).normalized;
        bulletTimer += Time.deltaTime;

        if (player.GetButtonDown("ChangeRes"))  GameMaster.me.ChangeResolution();
        if (player.GetButtonDown("ToggleMusic")) GameMaster.me.toggleMusic();

        if (player.GetButtonDown("Start")) {
            if (GameMaster.me.GameIsPaused && !GameMaster.me.countingDown) {
                GameMaster.me.StartCoroutine(GameMaster.me.Countdown(1));
            } else if (!GameMaster.me.GameIsPaused && (!gameOver && (otherPlayer == null || !otherPlayer.gameOver))) {
                GameMaster.me.Pause();
            }

            if (GameMaster.me.matchOver) {
                GameMaster.me.matchOver = false;
                GameMaster.me.resetScores();
                Time.timeScale = 1f;
            }
        }

        if (dir != Vector2.zero) prevDir = dir;

        if (player.GetButtonDown("Jump") && jumpChargeTimer < jumpChargeMax) jumpChargeTimer++;

        if (player.GetButtonDown("Jump")) {
            jumpTimer = 5;
            if (grounded) {
                wallCast();
                if (!onWall) GameMaster.me.SpawnParticle(jumpParticle, (Vector2)transform.position + (Vector2.down * .1f), playerColor);
                else         GameMaster.me.SpawnParticle(wallParticle, (Vector2)transform.position + (Vector2.down * .1f), playerColor);
            }
        }

        if (down && vel.y < 0 && !fastfall) fastfall = true;

        if (player.GetButtonDown("Shoot") && canShoot()) {
            shootBullet();
        } else if (player.GetButtonDown("Shoot") && !canShoot()) {
            SoundController.me.PlaySoundAtPitch(cantShootSound, .6f, 0.25f);
        }

        if (player.GetButton("SlowTime") && (canSlowTime() || (slow && mana > 0))) {
            if (!slow && (otherPlayer == null || !otherPlayer.slow))
                SoundController.me.PlaySound(slowSound, .5f);
            slow = true;
            mana -= timeManaDrain;
        } else {
            slow = false;
            if (mana < maxMana) mana += manaGain;
        }

        // Push slow state to network so TimeManager on all machines sees it.
        if (IsSpawned && IsOwner) netSlow.Value = slow;

        if (player.GetButtonDown("Restart") && (gameOver || (otherPlayer != null && otherPlayer.gameOver))
                && !GameMaster.me.matchOver && GameMaster.me.roundOver) {
            GameMaster.me.roundOver = false;
            Time.timeScale = 1f;
            string[] levels = { "FINAL_level1","FINAL_level2","FINAL_level3",
                                 "FINAL_level4","FINAL_level5","FINAL_level6","FINAL_level7" };
            string next = levels[Random.Range(0, levels.Length)];
            if (IsSpawned && IsServer) {
                NetworkManager.Singleton.SceneManager.LoadScene(next, UnityEngine.SceneManagement.LoadSceneMode.Single);
            } else if (!IsSpawned) {
                UnityEngine.SceneManagement.SceneManager.LoadScene(next);
            }
        }

        if (almostdead) almostDeadCircle.enabled = true;
        if (mana < 0)   mana = 0;

        handleInvuln();
        checkGameOver();
    }

    void handleInvuln() {
        if (invuln && invulnCounter < invulnMaxFrames) invulnCounter++;
        else invuln = false;
    }

    void checkGameOver() {
        if (health <= 0 && !gameOver) {
            gameOver = true;
            Camera.main.GetComponent<CamControl>().enabled = false;

            // State-mutating side-effects are server-only in online play.
            if (!IsSpawned || IsServer) {
                if (otherPlayer != null) otherPlayer.health += 10;
                GameMaster.me.redWins  = 0;
                GameMaster.me.blueWins = 0;
            }

            GameMaster.me.updateUI();
            CamGameOver cameraController = camera.GetComponent<CamGameOver>();
            ammoText.gameObject.SetActive(false);
            camera.GetComponent<Screenshake>().enabled = false;
            reticle.gameObject.SetActive(false);
            camera.transform.position = new Vector3(transform.position.x, transform.position.y - .5f, -10);
            cameraController.playerWon = otherPlayer;
            cameraController.textColor = playerColor;
            cameraController.enabled = true;
            Time.timeScale = 1;
        }
    }

    // -----------------------------------------------------------------------

    void FixedUpdate() {
        // Remote players' positions arrive via NetworkTransform; skip local physics.
        if (IsSpawned && !IsOwner) return;

        setGrounded();
        wallCast();

        float desYScale = defaultScale.y + (Mathf.Abs(vel.y) * 0.02f);
        scaleSpd += ((desYScale - pivot.transform.localScale.y) * 3f * Time.fixedDeltaTime * 60f);
        scaleSpd *= 5.4f * Time.fixedDeltaTime;

        float accel = runAccel;
        float mx    = runMaxSpeed;

        if (!spinning) {
            pivot.transform.localScale = new Vector2(defaultScale.x + (defaultScale.y - pivot.transform.localScale.y), pivot.transform.localScale.y + scaleSpd);
        }

        if (!grounded && spinning) {
            sprite.eulerAngles = new Vector3(0, 0, sprite.eulerAngles.z + (spinSpd * Time.fixedDeltaTime * spinDir));
            pivot.transform.localScale = defaultScale;
        } else {
            sprite.eulerAngles = Vector3.zero;
        }

        if (jumpTimer > 0 && grounded) {
            if (left || right) {
                spinning = true;
                transform.localScale = defaultScale;
                if (left) spinDir = 1;
                if (right) spinDir = -1;
            }
            vel.y = jumpSpd;
            SoundController.me.PlaySoundAtNormalPitch(jumpSound, .1f, Mathf.Clamp(transform.position.x, -1, 1));
            jumpChargeTimer = 0;
        }

        if (onWall && !grounded) {
            spinning = false;
            vel.y *= wallFriction;
            if (onWallLeft)  vel.x = Mathf.Max(vel.x, 0);
            if (onWallRight) vel.x = Mathf.Min(vel.x, 0);
            if (jumpTimer > 0) {
                vel.x = onWallLeft ? 15f : -15f;
                vel.y = jumpSpd;
                GameMaster.me.SpawnParticle(wallParticle, (Vector2)transform.position + (Vector2.down * .1f), playerColor);
                onWall = false;
            }
        }

        if (!grounded) {
            vel.y -= gravity * Time.fixedDeltaTime;
            accel  = airAccel;
        }

        if (vel.y > 0 && !player.GetButton("Jump")) vel.y -= bonusGravity * Time.fixedDeltaTime;

        if (right && (!slow || grounded)) { vel.x += accel * Time.fixedDeltaTime; face =  1; }
        if (left  && (!slow || grounded)) { vel.x -= accel * Time.fixedDeltaTime; face = -1; }

        vel.x = Mathf.Clamp(vel.x, -mx, mx);

        if (!left && !right && grounded) {
            if (Mathf.Abs(vel.x) < groundDrag * Time.fixedDeltaTime) vel.x = 0;
            else vel.x -= (groundDrag * Mathf.Sign(vel.x)) * Time.fixedDeltaTime;
        }

        jumpTimer--;
        shotTimer--;

        if (gameOver) vel = Vector2.zero;

        if (Mathf.Abs(transform.position.x) > maxMapX) {
            transform.position = transform.position.x > 0
                ? new Vector2(-transform.position.x + .25f, transform.position.y)
                : new Vector2(-transform.position.x - .25f, transform.position.y);
        }

        if (transform.position.y > maxMapY)
            transform.position = new Vector2(transform.position.x, -transform.position.y + .25f);

        if (transform.position.y < minMapY)
            transform.position = new Vector2(transform.position.x, maxMapY - .25f);

        prevVel = vel;
        rb.MovePosition((Vector2)transform.position + vel * Time.fixedDeltaTime);

        Vector2 retVect = new Vector2(shootPt.transform.position.x + (dir.x * .5f), shootPt.transform.position.y + (dir.y * .5f));
        reticle.transform.position = retVect;
        reticle.transform.eulerAngles = new Vector3(0, 0, Geo.ToAng(dir));
    }

    // -----------------------------------------------------------------------
    // Shooting
    // -----------------------------------------------------------------------

    void shootBullet() {
        bulletTimer = 0;

        float ang = Geo.ToAng(dir) + 180;
        Vector2 spawnPos = dir.sqrMagnitude > 0
            ? new Vector2(shootPt.transform.position.x + dir.x * .5f, shootPt.transform.position.y + dir.y * .5f)
            : new Vector2(shootPt.transform.position.x + prevDir.x * .5f, shootPt.transform.position.y + prevDir.y * .5f);
        Vector2 shootDir = dir.sqrMagnitude > 0 ? dir : prevDir;

        // Immediate local effects (VFX, sound, kickback).
        Instantiate(muzzleFlash,   spawnPos, Quaternion.Euler(new Vector3(360 - ang, 90, 0)));
        Instantiate(shootParticle, spawnPos, Quaternion.Euler(new Vector3(360 - ang, 90, 0)));
        reticleScale.Play();

        if (slow && otherPlayer != null && otherPlayer.slow)
            SoundController.me.PlaySoundAtNormalPitch(shootSoundSlow, 1f, transform.position.x);
        else
            SoundController.me.PlaySoundAtNormalPitch(shootSound, 1f, transform.position.x);

        vel -= shootDir * kick;
        screenshake.SetScreenshake(.3f, .1f, shootDir);

        if (IsSpawned) {
            // Online: ask server to spawn and decrement bullets.
            SpawnBulletServerRpc(spawnPos, shootDir);
        } else {
            // Local play: instantiate directly.
            GameObject tempBullet = Instantiate(bullet, spawnPos, Quaternion.identity);
            tempBullet.GetComponent<Bullet>().vel = shootDir;
            amountOfBullets--;
        }

        updateUI();
    }

    [ServerRpc]
    void SpawnBulletServerRpc(Vector2 pos, Vector2 direction) {
        if (netBullets.Value <= 0) return;
        netBullets.Value--;

        GameObject tempBullet = Instantiate(bullet, pos, Quaternion.identity);
        tempBullet.GetComponent<NetworkObject>().Spawn(true);
        tempBullet.GetComponent<Bullet>().vel = direction;
    }

    // -----------------------------------------------------------------------
    // Respawn — called from Bullet when it hits this player.
    // Must be called on the server (or locally in offline play).
    // -----------------------------------------------------------------------

    public void respawn() {
        health--;

        if (IsSpawned && IsServer) netHealth.Value = health;

        if (health == 1) almostdead = true;

        if (health > 0 && !gameOver) {
            if (otherPlayer != null) {
                GameMaster.me.addToScore(otherPlayer.colorName, 1);
                otherPlayer.score++;
            }
            GameMaster.me.updateUI();

            Instantiate(hitParticle, transform.position, Quaternion.identity);

            if (IsSpawned && IsServer) {
                netBullets.Value++;   // give a bullet back

                var targetParams = new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
                };
                DisableForRespawnClientRpc(targetParams);
                StartCoroutine(ServerRespawnCoroutine());
            } else {
                // Local play: existing SetActive approach.
                invuln = true;
                amountOfBullets++;
                invulnCounter = 0;
                pivot.transform.localScale *= .2f;
                scaleSpd = 1;
                slow = false;
                reticle.transform.localScale = new Vector3(0.149699f, 0.149699f, 0.149699f);
                gameObject.SetActive(false);
                GameMaster.me.StartCoroutine(GameMaster.me.ReEnablePlayer(gameObject, otherPlayer?.gameObject));
            }

            GameMaster.me.StartCoroutine(GameMaster.me.rumble(this, 10f, .5f));
            if (otherPlayer != null)
                GameMaster.me.StartCoroutine(GameMaster.me.rumble(otherPlayer, .2f, .1f));
        }
    }

    // Server: wait, find spawn point, re-enable owner.
    IEnumerator ServerRespawnCoroutine() {
        yield return new WaitForSeconds(.9f);
        Vector2 point = GameMaster.me.getFarthestSpawnPoint(
            otherPlayer != null ? (Vector2)otherPlayer.transform.position : Vector2.zero);

        // Reposition server-side copy.
        transform.position = point;
        box.enabled = true;
        rb.simulated = true;

        var targetParams = new ClientRpcParams {
            Send = new ClientRpcSendParams { TargetClientIds = new[] { OwnerClientId } }
        };
        EnableAtPositionClientRpc(point, targetParams);
    }

    // Owner: disable visuals + physics while waiting to respawn.
    [ClientRpc]
    void DisableForRespawnClientRpc(ClientRpcParams rpcParams = default) {
        SetPlayerActive(false);
        invuln = true;
        invulnCounter = 0;
        slow = false;
        pivot.transform.localScale *= .2f;
        scaleSpd = 1;
        reticle.transform.localScale = new Vector3(0.149699f, 0.149699f, 0.149699f);
    }

    // Owner: teleport to spawn point and re-enable.
    [ClientRpc]
    void EnableAtPositionClientRpc(Vector2 pos, ClientRpcParams rpcParams = default) {
        transform.position = pos;
        SetPlayerActive(true);
    }

    // Called from Bullet (server-side) when a decayed bullet is picked up.
    [ClientRpc]
    public void GiveBulletClientRpc(ClientRpcParams rpcParams = default) {
        if (!IsSpawned) return;
        // Server already incremented netBullets; just refresh local field + UI.
        amountOfBullets = netBullets.Value;
        updateUI();
    }

    // Enables or disables the player's visual + physical presence without SetActive
    // (SetActive would stop Update and break NetworkVariable sync).
    void SetPlayerActive(bool active) {
        sprite.gameObject.SetActive(active);
        box.enabled = active;
        rb.simulated = active;
        reticle.SetActive(active);
        ammoText.gameObject.SetActive(active);
        if (almostDeadCircle != null) almostDeadCircle.enabled = active && almostdead;
    }

    // -----------------------------------------------------------------------

    public void updateUI() {
        ammoText.text = "" + (IsSpawned ? netBullets.Value : amountOfBullets);
    }

    bool canSlowTime() {
        return mana - timeManaDrain >= 2;
    }

    bool canShoot() {
        int bullets = IsSpawned ? netBullets.Value : amountOfBullets;
        return bullets > 0 && bulletTimer > bulletCooldown
            && !GameMaster.me.GameIsPaused && !GameMaster.me.countingDown && !gameOver;
    }

    // -----------------------------------------------------------------------
    // Physics callbacks
    // -----------------------------------------------------------------------

    void setGrounded() {
        Vector2 pt1 = transform.TransformPoint(box.offset + new Vector2(box.size.x / 2, -box.size.y / 2) + new Vector2(-.01f, 0));
        Vector2 pt2 = transform.TransformPoint(box.offset - (box.size / 2) + new Vector2(.01f, 0));
        prevGrounded = grounded;
        grounded = Physics2D.OverlapArea(pt1, pt2, LayerMask.GetMask("Platform")) != null;

        if (grounded) {
            spinning = false;
            if (!prevGrounded) scaleSpd = -13f * (Mathf.Abs(prevVel.y) / 30f);
            vel.y = 0;
            fastfall = false;
        }
    }

    void wallCast() {
        int mask = LayerMask.GetMask("Platform");
        Vector2 top = (Vector2)transform.position + box.offset + (Vector2.up * (box.size.y / 2f));
        Vector2 bot = (Vector2)transform.position + box.offset - (Vector2.up * (box.size.y / 2f));
        onWallLeft  = Physics2D.Raycast(top, Vector2.left,  box.size.x * .6f, mask) || Physics2D.Raycast(bot, Vector2.left,  box.size.x * .6f, mask);
        onWallRight = Physics2D.Raycast(top, Vector2.right, box.size.x * .6f, mask) || Physics2D.Raycast(bot, Vector2.right, box.size.x * .6f, mask);
        onWall = onWallLeft || onWallRight;
    }

    void OnCollisionEnter2D(Collision2D coll) {
        if (coll.contacts.Length == 0) return;

        ContactPoint2D pt = coll.contacts[0];

        if (coll.gameObject.layer == LayerMask.NameToLayer("Platform")) {
            // Physical response — run on owner only.
            if (ShouldProcessInput) {
                vel += pt.normal * Vector2.Dot(-pt.normal, vel);
                if (!prevGrounded) {
                    wallCast();
                    if (!onWall) GameMaster.me.SpawnParticle(landParticle, coll.contacts[0].point, playerColor, coll.gameObject.GetComponent<SpriteRenderer>().color);
                    else         GameMaster.me.SpawnParticle(wallParticle, (Vector2)transform.position + (Vector2.down * .8f), playerColor);
                }
            }
        }

        if (coll.gameObject.tag == "Player") {
            // Bounce — physical response, owner only.
            if (ShouldProcessInput) vel.y = jumpSpd;

            // Bullet steal — server (or local play) only.
            bool isServer = !IsSpawned || IsServer;
            if (isServer && GameMaster.me.bulletRecentlyStolenTimer > 120) {
                PlayerMovementController other = coll.gameObject.GetComponent<PlayerMovementController>();
                int myBullets    = IsSpawned ? netBullets.Value        : amountOfBullets;
                int theirBullets = IsSpawned ? other.netBullets.Value  : other.amountOfBullets;
                if (myBullets < theirBullets) {
                    if (IsSpawned) {
                        netBullets.Value++;
                        other.netBullets.Value--;
                    } else {
                        amountOfBullets++;
                        other.amountOfBullets--;
                    }
                    GameMaster.me.bulletRecentlyStolenTimer = 0;
                }
            }
        }

        if (coll.gameObject.tag == "Bullet") {
            if (ShouldProcessInput) updateUI();
        }

        if (coll.gameObject.tag == "Pinata") { /* no-op */ }
    }

    void OnTriggerEnter2D(Collider2D coll) { }
}
