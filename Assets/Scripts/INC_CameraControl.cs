using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class INC_CameraControl : MonoBehaviour {

    public Camera targetCamera;
    public Text zoomText;
    public float Zoom = 60;

	// Use this for initialization
	void Start () {
		if (!targetCamera)
        {
            Debug.Log("There is no camera attached!");
        }
        else if (!zoomText)
        {
            Zoom = targetCamera.fieldOfView;
            Debug.Log("There is no text label attached!");
        }
        else
        {
            Zoom = targetCamera.fieldOfView;
            zoomText.text = Zoom.ToString() + "x";
        }
	}
	
	// Update is called once per frame
	void Update () {

    }

    public void ZoomIn()
    {
        if (targetCamera)
        {
            if (Zoom > 10)
            {
                Zoom -= 10;
                targetCamera.fieldOfView = Zoom;
            }
            setText();
        }
    }

    public void ZoomOut()
    {
        
        if (targetCamera)
        {
            if (Zoom < 130)
            {
                Zoom += 10;
                targetCamera.fieldOfView = Zoom;
            }
            setText();
        }
    }

    void setText()
    {
        zoomText.text = Zoom.ToString() + "x";
    }
}
