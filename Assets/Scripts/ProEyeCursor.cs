using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProEyeCursor : MonoBehaviour
{

    public GameGaze gaze;
    public GameObject cursor;
    public GameObject lighter;
    float x, y;
    // Use this for initialization
    void Start()
    {
        if (cursor == null)
        {
            cursor = GameObject.Find("Image");
        }
        if (gaze == null)
        {
            gaze = GameObject.Find("Gaze").GetComponent<GameGaze>();
        }
        if (lighter == null)
        {
            lighter = GameObject.Find("LightEye");
        }
    }

    // Update is called once per frame
    void Update()
    {
        gaze.GetPos(lighter.gameObject, ref x, ref y);
        //cursor.transform.localPosition = new Vector2((x - 0.5f) * 1512, (0.5f - y) * 1680);
    }
}
