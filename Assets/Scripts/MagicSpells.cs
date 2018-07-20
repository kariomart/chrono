using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagicSpells : MonoBehaviour {


//	static def ToAng (a as Vector2):
//	return Mathf.Atan2(a.y, a.x) * Mathf.Rad2Deg
//
//		static def ToAng(a as Vector2, b as Vector2):
//		return ToAng(b - a)
//
//
//			static def ToVect (a as single):
//			return Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad))


	// Use this for initialization
	void Start () {
		
	}


	public static float ToAng(Vector2 a){

		return Mathf.Atan2 (a.y, a.x) * Mathf.Rad2Deg;

	}


	public static float ToAng(Vector2 a, Vector2 b) {
		return ToAng (b - a);

	}


	public static Vector2 ToVect(float a){

		return new Vector2 (Mathf.Cos (a * Mathf.Deg2Rad), Mathf.Sin (a * Mathf.Deg2Rad));

	}





	// Update is called once per frame
	void Update () {
		
	}
}
