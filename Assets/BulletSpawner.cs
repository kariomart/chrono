using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletSpawner : MonoBehaviour {

	public int spawnChance;
	public GameObject bullet;
	public PlayerMovementController player1;
	public static int totalBullets;

	// Use this for initialization
	void Start () {

		totalBullets +=  player1.amountOfBullets;
		totalBullets +=  player1.otherPlayer.amountOfBullets;
		
	}
	
	// Update is called once per frame
	void Update () {
		
		int rand = Random.Range(0, spawnChance + (10 * totalBullets));

		if (rand == 2) {
			SpawnBullet();
		}

	}


	void SpawnBullet() {

		Instantiate(bullet, transform.position, Quaternion.identity);

	}
}
