using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeLord : MonoBehaviour
{
    SpriteRenderer inside;
    GameObject clock;
    bool slowLast;
    // Start is called before the first frame update
    void Start()
    {
        inside = transform.GetChild(0).gameObject.GetComponent<SpriteRenderer>();
        clock = transform.GetChild(1).gameObject;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        float ang = clock.transform.localEulerAngles.z;
        ang -=  774 * Time.fixedDeltaTime;
        clock.transform.localEulerAngles = new Vector3(0, 0, ang);
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        if (coll.gameObject.tag == "bullet")
        {
            float duration = Random.Range(1f, 5f);
            StopAllCoroutines();
            GameMaster.me.timeMaster.TimeLord(slowLast, duration);
            slowLast = !slowLast;
            //StartCoroutine(TimeLordSlow(duration));
            //StartCoroutine(RotateClock(duration));
        }
    }

    IEnumerator TimeLordSlow(float time)
    {
        Color c = inside.color;
        float s = 0.7138506f;
        inside.color = new Color(c.r, c.g, c.b, 1);

        for (float i = 0; i < 25; i++)
        {
            float scale = Mathf.Lerp(0, s, i / 25);
            inside.transform.localScale = new Vector3(scale, scale, scale);
            yield return new WaitForSecondsRealtime(.0001f);    
        }


        for (float i = 0; i < time * 60; i++)
        {
            inside.color = new Color(c.r, c.g, c.b, 1 - i / (time*60));
            yield return new WaitForSecondsRealtime(1f / 60f);
        }
    }

    IEnumerator RotateClock(float time)
    {

        for (float i = 0; i < time * 60; i++)
        {
            float ang = Mathf.Lerp(0, 360, i / (time * 60f));
            ang = (int)ang;
            Debug.Log(ang);
            clock.transform.localEulerAngles = new Vector3(0, 0, -ang);            
            yield return new WaitForSecondsRealtime(1f / 60f);
        }

    }
}
