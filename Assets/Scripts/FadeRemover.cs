using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeRemover : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject camera;
    public bool done = false;

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (!done && camera != null)
        {
            if (camera.GetComponent<SteamVR_Fade>())
            {
                camera.GetComponent<SteamVR_Fade>().enabled = false;
                done = true;
            }
        }
    }
}
