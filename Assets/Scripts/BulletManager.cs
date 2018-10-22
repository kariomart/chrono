using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {
	public static BulletManager me;
	public List<BulletSpawner> spawnList = new List<BulletSpawner>();
	public GameObject bullet;
	public static int amtBullets;


	void Awake(){
		me = this;
		BulletManager.amtBullets += 4;
		StartCoroutine(SpawnBullets());
	}

	IEnumerator SpawnBullets(){
		while (true){
			yield return new WaitForSeconds(Random.Range(amtBullets, 4 * amtBullets));
			SpawnBullet();

		}
	}

	void SpawnBullet() {
		int k = Random.Range(0, spawnList.Count);
		
		for (int i = 0; i < spawnList.Count; i++) {
			int j = (i + k) % spawnList.Count;
			if (spawnList[j].myBullet == null){
				spawnList[j].myBullet = Instantiate(bullet, spawnList[j].transform.position, Quaternion.identity);
				Bullet b = spawnList[j].myBullet.GetComponent<Bullet>();
				b.decayed = true;
				b.lifetime = 10f;
				amtBullets ++;
				break;
			}
		}
	}
}
