using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;

public class Reset : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject camera;
    public float startTime;
    public float delay = 0.2f;
    void Start()
    {
        startTime = Time.time;
    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time < startTime + delay)
        {
            // reset position 
            Valve.VR.OpenVR.Compositor.SetTrackingSpace(Valve.VR.ETrackingUniverseOrigin.TrackingUniverseSeated);
            Valve.VR.OpenVR.System.ResetSeatedZeroPose();
        }
    }
}
