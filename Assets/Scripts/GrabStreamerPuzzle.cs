using System.Collections;
using System.Collections.Generic;
using LSL;
using NewtonVR;
using UnityEngine;
using VRTK;

public class GrabStreamerPuzzle : MonoBehaviour {

    private liblsl.StreamOutlet markerStream;
    public VRTK_InteractGrab left;
    public VRTK_InteractGrab right;

	// Use this for initialization
	void Start ()
    {
        liblsl.StreamInfo inf =
            new liblsl.StreamInfo("GrabMarker", "Markers", 1, 0, liblsl.channel_format_t.cf_string, "controller");
        markerStream = new liblsl.StreamOutlet(inf);
        if (left)
        {
            left.ControllerStartGrabInteractableObject += Left_ControllerStartGrabInteractableObject;
        }

        if (right)
        {
            right.ControllerStartGrabInteractableObject += Right_ControllerStartGrabInteractableObject;
        }
	}

    private void Right_ControllerStartGrabInteractableObject(object sender, ObjectInteractEventArgs e)
    {
        string name = "";
        //if (interactable.GetComponent<UniqueID>())
        //    name = interactable.GetComponent<UniqueID>().guid;
        //else 
        name = e.target.name;
        string markerString = "Right hand grabs:" + name;
        Debug.Log(markerString);
        string[] tempSample = { markerString };
        markerStream.push_sample(tempSample);
    }

    private void Left_ControllerStartGrabInteractableObject(object sender, ObjectInteractEventArgs e)
    {
        string name = "";
        //if (interactable.GetComponent<UniqueID>())
        //    name = interactable.GetComponent<UniqueID>().guid;
        //else 
        name = e.target.name;
        string markerString = "Left hand grabs:" + name;
        Debug.Log(markerString);
        string[] tempSample = { markerString };
        markerStream.push_sample(tempSample);
    }

    public void SendGrabMarkerLeft(NVRInteractable interactable)
    {
        string name = "";
        //if (interactable.GetComponent<UniqueID>())
        //    name = interactable.GetComponent<UniqueID>().guid;
        //else 
            name = interactable.name;
        string markerString = "Left hand grabs:" + name;
        Debug.Log(markerString);
        string[] tempSample = { markerString };
        markerStream.push_sample(tempSample);
    }

    public void SendGrabMarkerRight(NVRInteractable interactable)
    {
        string name = "";
        //if (interactable.GetComponent<UniqueID>())
        //    name = interactable.GetComponent<UniqueID>().guid;
        //else 
            name = interactable.name;
        string markerString = "Right hand grabs:" + name;
        Debug.Log(markerString);
        string[] tempSample = { markerString };
        markerStream.push_sample(tempSample);
    }

    public void SendTeleportationEvent(bool start, Vector3 pos)
    {
        string[] tempSample;
        if (start)
        {
            tempSample = new string[] { "Start of teleportation:" + pos.ToString("F4") };
        }
        else
        {
            tempSample = new string[] { "End of teleportation:" + pos.ToString("F4") };
        }
        Debug.Log(pos);
        markerStream.push_sample(tempSample);
    }

}
