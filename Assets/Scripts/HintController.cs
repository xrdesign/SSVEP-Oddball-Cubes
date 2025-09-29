using System.Collections;
using System.Collections.Generic;
using NewtonVR;
using UnityEngine;

public class HintController : MonoBehaviour
{
    public ItemSpawner itemSpawner;

    public GameObject screenFade;
    public GameObject target;

    public bool showingHint = false;

    public GameObject head;

    public NVRHand leftHand;
    public NVRHand rightHand;

    public LSLStreamer lslStreamer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void ShowHint()
    {
        showingHint = true;
    }

    public void HideHint()
    {
        showingHint = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (leftHand.UseButtonDown || rightHand.UseButtonDown)
        {
            itemSpawner.hintShown = true;
            ShowHint();

            if (!target)
            {
                lslStreamer.SendEvent("Enable Hint");
            }
        }
        
        if (leftHand.UseButtonUp || rightHand.UseButtonUp)
        {
            if (target)
            {
                lslStreamer.SendEvent("Disable Hint");
            }
            HideHint();
        }

        if (leftHand.HoldButtonDown)
        {
            lslStreamer.SendEvent("Left trigger is pressed.");
        }

        if (leftHand.HoldButtonUp)
        {
            lslStreamer.SendEvent("Left trigger is released.");
        }

        if (rightHand.HoldButtonDown)
        {
            lslStreamer.SendEvent("Right trigger is pressed.");
        }

        if (rightHand.HoldButtonUp)
        {
            lslStreamer.SendEvent("Right trigger is released.");
        }

        if (showingHint)
        {
            // instantiate a copy of target
            if (!target)
            {
                target = Instantiate(itemSpawner.target, head.transform.position + head.transform.forward * 0.5f,
                    Quaternion.identity);

                // set the rigidbody to kinematic if target has one
                if (target.GetComponent<Rigidbody>())
                {
                    target.GetComponent<Rigidbody>().isKinematic = true;
                }

                if (target.GetComponent<Renderer>())
                {
                    target.GetComponent<Renderer>().enabled = true;
                }

                // enable renderer in children as well
                var list = target.GetComponentsInChildren<Renderer>();
                foreach (var r in list)
                {
                    r.enabled = true;
                }
            }
        }
        else
        {
            DestroyImmediate(target);
            target = null;
        }
    }
}
