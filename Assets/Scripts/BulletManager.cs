using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class BulletManager : MonoBehaviour {

    public static BulletManager me;
    public List<BulletSpawner> spawnList = new List<BulletSpawner>();
    public GameObject bullet;
    public static int amtBullets;

    void Awake() {
        me = this;
        BulletManager.amtBullets += 4;
        StartCoroutine(SpawnBullets());
    }

    IEnumerator SpawnBullets() {
        while (true) {
            yield return new WaitForSeconds(Random.Range(1 + (amtBullets / 3f), 2 + (amtBullets / 2f)));
            // Only the server (or local play) spawns pickup bullets.
            if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening || NetworkManager.Singleton.IsServer)
                SpawnBullet();
        }
    }

    void SpawnBullet() {
        int k = Random.Range(0, spawnList.Count);
        for (int i = 0; i < spawnList.Count; i++) {
            int j = (i + k) % spawnList.Count;
            if (spawnList[j].myBullet == null) {
                spawnList[j].myBullet = Instantiate(bullet, spawnList[j].transform.position, Quaternion.identity);

                // In online play, spawn as a NetworkObject so clients see it.
                NetworkObject no = spawnList[j].myBullet.GetComponent<NetworkObject>();
                if (no != null && NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
                    no.Spawn(true);

                Bullet b = spawnList[j].myBullet.GetComponent<Bullet>();
                GameMaster.me.SpawnParticle(b.bulletSpawned, spawnList[j].transform.position);
                b.decayed  = true;
                b.lifetime = 99999999f;
                amtBullets++;
                break;
            }
        }
    }
}
