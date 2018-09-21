using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour {

	public int spawnChance;
	public GameObject bullet;
	public PlayerMovementController player1;
	static int totalBullets;

	// Use this for initialization
	void Start () {

		player1 = GameObject.Find("red").GetComponent<PlayerMovementController>();
		
	}
	
	// Update is called once per frame
	void Update () {
		
		Debug.Log(spawnChance + (150 * totalBullets));
		int rand = Random.Range(0, spawnChance + 400 + (150 * totalBullets));

		if (rand == 2) {
			SpawnBullet();
		}

	}


	void SpawnBullet() {

		Instantiate(bullet, transform.position, Quaternion.identity);
		totalBullets ++;

	}
}
