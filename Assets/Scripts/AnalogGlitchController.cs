using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Kino;


public class AnalogGlitchController : MonoBehaviour {

	AnalogGlitch glitch;


	public int scalineJitterChance;
	public float scalineMax;
	public bool scanlining;

	public int verticalJumpChance;
	public bool jumping;

	public int horizontalShakeChance;
	public bool shaking;

	public int colorDriftChance;
	public float colorDriftMax;
	public bool drifting;

	public bool glitchFX;



	// Use this for initialization
	void Start () {

		glitch = GetComponent<AnalogGlitch>();
		
	}
	
	// Update is called once per frame
	void Update () {

		if (Input.GetKeyDown(KeyCode.Alpha0)) { 
			glitchFX = !glitchFX;
		}
		
	}

	void FixedUpdate() {

		if (glitchFX) {

			int rand;

			rand = Random.Range(0, scalineJitterChance);

			if (rand == 10 && !scanlining) {
				scalineMax = Random.Range(0, .2f);
				//glitch.scanLineJitter = Random.Range(0, .2f);
				scanlining = true;
			}

			if (scanlining && glitch.scanLineJitter <= scalineMax) {
				glitch.scanLineJitter += 0.001f;
			}

			if (glitch.scanLineJitter >= scalineMax) {
				glitch.scanLineJitter -= 0.001f;
				scanlining = false;
				scalineMax = 0f; 
			} 

			if (glitch.scanLineJitter <= 0) {
				glitch.scanLineJitter = 0;
			}


			rand = Random.Range(0, verticalJumpChance);

			if (rand == 10 && !jumping) {
				glitch.verticalJump = Random.Range(0, .3f);
				jumping = true;
			}

			if (jumping && glitch.verticalJump > 0) {
				glitch.verticalJump -= 0.005f;
			} else {
				jumping = false;
				glitch.verticalJump = 0;
			}


			rand = Random.Range(0, horizontalShakeChance);

			if (rand == 10 && !shaking) {
				glitch.horizontalShake = Random.Range(0, .15f);
				shaking = true;
			}

			if (shaking && glitch.horizontalShake > 0) {
				glitch.horizontalShake -= 0.001f;
			} else {
				shaking = false;
				glitch.horizontalShake = 0;
			}

			rand = Random.Range(0, colorDriftChance);

			if (rand == 10 && !drifting) {
				colorDriftMax = Random.Range(0, .1f);
				//glitch.scanLineJitter = Random.Range(0, .2f);
				drifting = true;
			}

			if (drifting && glitch.colorDrift <= colorDriftMax) {
				glitch.colorDrift += 0.001f;
			}

			if (glitch.colorDrift >= colorDriftMax) {
				glitch.colorDrift -= 0.001f;
				drifting = false;
				colorDriftMax = 0f; 
			} 

			if (glitch.colorDrift <= 0) {
				glitch.colorDrift = 0;
			}
		}
		

		


	}
}
