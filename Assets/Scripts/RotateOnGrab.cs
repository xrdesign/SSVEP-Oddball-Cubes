using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewtonVR;

public class RotateOnGrab : NVRInteractable
{
    public Vector3 rotationAxis;
    public float rotationAngle = 30;

    public override void BeginInteraction(NVRHand hand)
    {
        base.BeginInteraction(hand);
        transform.Rotate(rotationAxis, rotationAngle);
    }
}
