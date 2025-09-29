using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NewtonVR;

public class Activate_on_grab : NVRInteractable
{
    public bool active = false;

    private bool triggered = false;
    private MeshRenderer meshRenderer;
    private Material material;
    private float _targetAlpha = 0;
    private float timer = 0;

    public override void BeginInteraction(NVRHand hand)
    {
        base.BeginInteraction(hand);
        triggered = true;
        gameObject.SetActive(active);
    }
}
