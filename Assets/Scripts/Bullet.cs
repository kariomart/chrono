using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class Bullet : NetworkBehaviour {

    public float defaultSpd;
    public float spd;
    public int dmg;
    public Rigidbody2D rb;
    public SpriteRenderer sprite;
    public BoxCollider2D pickupBox;
    float spawnTime;
    float nonDecayedTime;
    public float decayTime;
    public bool decayed;
    public float decayVel;
    public Vector2 vel;
    public float maxSpd;
    public int bounceCount;
    public float decayDeath;
    public float decayDeathCounter;
    public float lifetime;
    public bool slowZoneAccel;

    public ParticleSystem hitObjectEffect;
    public ParticleSystem shoot;
    public ParticleSystem hitPlayerEffect;
    public ParticleSystem trail;
    public AudioClip playerHit;
    public AudioClip lastHit;
    public AudioClip bounce1;
    public AudioClip bounce2;
    public AudioClip bounce3;
    public AudioClip tick;

    public GameObject decayEffect;
    public ParticleSystem middle;
    public ParticleSystem bulletWall;
    public ParticleSystem bulletSpawned;

    public Color decayColor = Color.grey;

    GameObject DamageFlash;
    Vector2 prevVel;
    public float maxMapX;
    public float maxMapY;
    public bool slowzone;
    int blinkCounter;

    bool IsServerOrLocal => !IsSpawned || IsServer;

    void Start() {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        spawnTime = Time.time;
        defaultSpd = spd;
        if (lifetime == 0) lifetime = Random.Range(3f, 5f);
    }

    void Update() {
        // Visual / lifetime logic runs on all machines.
        nonDecayedTime = Time.time - spawnTime;

        if (decayed) {
            decayDeathCounter += Time.deltaTime;
            if (decayDeathCounter > lifetime) {
                // Only the server destroys networked objects.
                if (IsServerOrLocal) {
                    BulletManager.amtBullets--;
                    NetworkObject no = GetComponent<NetworkObject>();
                    if (no != null && no.IsSpawned) no.Despawn(true);
                    else Destroy(gameObject);
                }
            }
        }

        if (lifetime - decayDeathCounter <= 1.5f && decayed) blinking();

        Color c = sprite.color;
        if (decayDeathCounter > 0)
            sprite.color = new Color(c.r, c.g, c.b, (lifetime / decayDeathCounter) / 10);
    }

    void FixedUpdate() {
        // In online mode, only the server moves bullets.
        if (!IsServerOrLocal) return;

        if (nonDecayedTime >= decayTime || decayed) {
            var main = middle.main;
            main.startColor = decayColor;
            pickupBox.enabled = true;
            decayed = true;
            trail.Stop();
        }

        if (Mathf.Abs(transform.position.x) > maxMapX) {
            transform.position = transform.position.x > 0
                ? new Vector2(-transform.position.x + .25f, transform.position.y)
                : new Vector2(-transform.position.x - .25f, transform.position.y);
            vel *= .8f;
        }

        if (Mathf.Abs(transform.position.y) > maxMapY) {
            transform.position = transform.position.y > 0
                ? new Vector2(transform.position.x, -transform.position.y + .25f)
                : new Vector2(transform.position.x, -transform.position.y - .25f);
            vel *= .8f;
        }

        if (spd < decayVel && !decayed && !slowzone) decayed = true;

        spd = Mathf.Clamp(spd, 0, maxSpd);
        rb.MovePosition((Vector2)transform.position + vel * spd * Time.fixedDeltaTime);
        prevVel = vel;
    }

    void ParticleEffect(GameObject obj) {
        Instantiate(hitObjectEffect, transform.position, Quaternion.identity);
        var main = hitObjectEffect.main;
        main.startColor = Color.grey;
        SpriteRenderer sr = obj.GetComponent<SpriteRenderer>() ?? obj.GetComponentInChildren<SpriteRenderer>();
        if (sr != null) main.startColor = sr.color;
    }

    void OnTriggerEnter2D(Collider2D coll) {
        // SlowZone and Juicer logic is server-authoritative in online play.
        if (!IsServerOrLocal) return;

        if (coll.gameObject.layer == LayerMask.NameToLayer("SlowZone")) {
            slowzone = true;
            spd *= (GameMaster.me.player1 != null && GameMaster.me.player1.slow) ||
                   (GameMaster.me.player2 != null && GameMaster.me.player2.slow) ? 3f : .2f;
            decayed = true;
            trail.Play();
        }

        if (coll.gameObject.tag == "Juicer") {
            Debug.Log("Juiced");
            spd *= Random.Range(3f, 5f);
        }
    }

    void OnTriggerStay2D(Collider2D coll) {
        if (!IsServerOrLocal) return;

        if (coll.gameObject.layer == LayerMask.NameToLayer("SlowZone")) {
            if ((GameMaster.me.player1 != null && GameMaster.me.player1.slow) ||
                (GameMaster.me.player2 != null && GameMaster.me.player2.slow))
                slowZoneAccel = true;
            decayed = false;
        }

        if (coll.gameObject.tag == "pivot" && !decayed) {
            GameMaster.me.timeMaster.gameOverSlow = true;
            GameMaster.me.timeMaster.pos = coll.gameObject.transform.position;
            if (IsSpawned) SetGameOverSlowClientRpc(true);
        }
    }

    void OnTriggerExit2D(Collider2D coll) {
        if (!IsServerOrLocal) return;

        if (coll.gameObject.tag == "pivot" && !decayed) {
            GameMaster.me.timeMaster.gameOverSlow = false;
            if (IsSpawned) SetGameOverSlowClientRpc(false);
        }

        if (coll.gameObject.layer == LayerMask.NameToLayer("SlowZone")) {
            spd *= 6f;
            decayed = false;
            middle.Play();
            trail.Play();
        }
        slowzone = false;
    }

    void OnCollisionStay2D(Collision2D coll) {
        // All state-changing collision logic is server-only in online play.
        if (!IsServerOrLocal) return;

        if (coll.gameObject.tag == "Player") {
            PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController>();
            if (!player.invuln) {
                if (player.health == 1) SoundController.me.PlaySoundAtNormalPitch(lastHit, 1f);
                else                    SoundController.me.PlaySoundAtNormalPitch(playerHit, 1f, transform.position.x);

                player.respawn();
                GameMaster.me.addColorDrift();

                if (DamageFlash != null) {
                    GameObject flash = Instantiate(DamageFlash, transform.position, Quaternion.identity);
                    Camera.main.GetComponent<Screenshake>().SetScreenshake(0.35f, .25f, player);
                    Destroy(flash, .020f);
                }

                DestroyBullet();
            } else {
                if (coll.contacts.Length > 0)
                    vel = Geo.ReflectVect(prevVel.normalized, coll.contacts[0].normal) * (prevVel.magnitude * 0.65f);
            }
        }
    }

    void OnCollisionEnter2D(Collision2D coll) {
        // All state-changing collision logic is server-only in online play.
        if (!IsServerOrLocal) return;

        if (coll.gameObject.tag == "Player" && decayed) {
            PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController>();
            if (player.netBullets.Value < 10) {
                SoundController.me.PlaySound(tick, .6f, 1, transform.position.x);
                player.netBullets.Value++;

                // Notify the owning client to refresh its UI.
                var targetParams = new ClientRpcParams {
                    Send = new ClientRpcSendParams { TargetClientIds = new[] { player.OwnerClientId } }
                };
                if (player.IsSpawned) player.GiveBulletClientRpc(targetParams);

                DestroyBullet();
            }
            return;
        }

        if (coll.gameObject.tag == "Player" && !decayed) {
            PlayerMovementController player = coll.gameObject.GetComponent<PlayerMovementController>();
            if (!player.invuln) {
                if (player.health == 1) SoundController.me.PlaySoundAtNormalPitch(lastHit, 1f);
                else                    SoundController.me.PlaySoundAtNormalPitch(playerHit, 1f, transform.position.x);

                player.respawn();
                GameMaster.me.addColorDrift();

                if (DamageFlash != null) {
                    GameObject flash = Instantiate(DamageFlash, transform.position, Quaternion.identity);
                    Camera.main.GetComponent<Screenshake>().SetScreenshake(0.35f, .25f, player);
                    Destroy(flash, .020f);
                }

                DestroyBullet();
            } else {
                if (coll.contacts.Length > 0)
                    vel = Geo.ReflectVect(prevVel.normalized, coll.contacts[0].normal) * (prevVel.magnitude * 0.65f);
            }
            return;
        }

        if ((coll.gameObject.tag == "Stage" || coll.gameObject.tag == "Wall" || coll.gameObject.tag == "Pinata")
                && coll.contacts.Length > 0) {
            vel = Geo.ReflectVect(prevVel.normalized, coll.contacts[0].normal) * (prevVel.magnitude * 0.65f);
            bounceCount++;
            if (bounceCount >= 4) decayed = true;
            playBounceSound();

            SpriteRenderer wallSR = coll.gameObject.GetComponent<SpriteRenderer>();
            if (wallSR != null)
                GameMaster.me.SpawnParticle(bulletWall, coll.contacts[0].point, Color.white, wallSR.color);
        }

        if (coll.gameObject.tag == "bullet") {
            Bullet b = coll.gameObject.GetComponent<Bullet>();
            if (coll.contacts.Length > 0) {
                if (b.vel == Vector2.zero) {
                    b.vel = vel; b.prevVel = vel; b.spd = spd;
                } else {
                    vel = Geo.ReflectVect(prevVel.normalized, coll.contacts[0].normal) * (prevVel.magnitude * 0.65f);
                }
            }
            bounceCount++;
            if (bounceCount >= 4) decayed = true;
            playBounceSound();
            ParticleEffect(coll.gameObject);
        }

        if (coll.gameObject.tag == "Pinata") {
            Pinata pinata = coll.gameObject.GetComponent<Pinata>();
            pinata.health--;
            if (pinata.physics != null) pinata.physics.vel += -vel * 5f;
            pinata.Shrink();
            ParticleEffect(coll.gameObject);
        }
    }

    [ClientRpc]
    void SetGameOverSlowClientRpc(bool value) {
        GameMaster.me.timeMaster.gameOverSlow = value;
    }

    void DestroyBullet() {
        NetworkObject no = GetComponent<NetworkObject>();
        if (no != null && no.IsSpawned) no.Despawn(true);
        else Destroy(gameObject);
    }

    void blinking() {
        blinkCounter++;
        if (blinkCounter % 4 == 0)      middle.Stop(false, ParticleSystemStopBehavior.StopEmittingAndClear);
        else if (blinkCounter % 2 == 0) middle.Play();
    }

    void playBounceSound() {
        float vol = 1;
        if      (bounceCount == 1) SoundController.me.PlaySound(bounce1, vol, 1, transform.position.x);
        else if (bounceCount == 2) SoundController.me.PlaySound(bounce2, vol, 1, transform.position.x);
        else if (bounceCount >= 3) SoundController.me.PlaySound(bounce3, vol, 1, transform.position.x);
    }
}
