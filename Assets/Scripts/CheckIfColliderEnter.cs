using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckIfColliderEnter : MonoBehaviour {
    
    public bool isEnter;

	// Use this for initialization
	void Start () {
        isEnter = false;
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnTriggerEnter(Collider otherObject)
    {
        if (otherObject.gameObject.CompareTag("Player"))
        {
            isEnter = true;
        }
    }
}
