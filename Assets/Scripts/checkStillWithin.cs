using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VRTK.Examples.Archery;

public class checkStillWithin : MonoBehaviour {

    public Transform oriParent;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void checkIn()
    {
        if (GetComponent<BoxCollider>().enabled)
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Follow>().followPosition = false;
        }
    }

    public void checkOff()
    {
        if (!GetComponent<BoxCollider>().enabled)
        {
            GetComponent<BoxCollider>().enabled = true;
            
            
        }
    }
}
