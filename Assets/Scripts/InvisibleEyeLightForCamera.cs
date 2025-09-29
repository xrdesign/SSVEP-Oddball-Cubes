using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvisibleEyeLightForCamera : MonoBehaviour
{
    public bool enableVRRender;
    public Light limelight;

    void OnPreCull()
    {
        if (limelight != null && !enableVRRender)
            limelight.enabled = false;
    }

    void OnPreRender()
    {
        if (limelight != null && !enableVRRender)
            limelight.enabled = false;
    }
    void OnPostRender()
    {
        if (limelight != null && !enableVRRender)
            limelight.enabled = true;
    }
}
