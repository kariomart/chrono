﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour {
	public static BulletManager me;
	public List<BulletSpawner> spawnList = new List<BulletSpawner>();
	public GameObject bullet;
	//public static int amtBullets;


	void Awake(){
		me = this;
		StartCoroutine(SpawnBullets());
	}

	IEnumerator SpawnBullets(){
		while (true){
			yield return new WaitForSeconds(Random.Range(GameMaster.me.amtBullets, 4 * GameMaster.me.amtBullets));
			SpawnBullet();

		}
	}

	void SpawnBullet() {
		int k = Random.Range(0, spawnList.Count);
		
		for (int i = 0; i < spawnList.Count; i++) {
			int j = (i + k) % spawnList.Count;
			if (spawnList[j].myBullet == null){
				spawnList[j].myBullet = Instantiate(bullet, spawnList[j].transform.position, Quaternion.identity);
				spawnList[j].myBullet.GetComponent<Bullet>().lifetime = 10f;
				GameMaster.me.amtBullets ++;
				break;
			}
		}
	}
}
