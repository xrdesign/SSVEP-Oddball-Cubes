using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostSurvey : MonoBehaviour {

    GameObject panel;
    GameObject obj;
    // Use this for initialization
    void Start () {
        obj = (Resources.Load("SurveyPanel") as GameObject);
    }
	
	// Update is called once per frame
	void Update () {
		
	}

    public void post()
    {
        
        if ( obj != null) 
        {
            panel = Instantiate(obj) as GameObject;
            panel.transform.parent = GameObject.Find("Head").transform;
        }
        else
        {
            print("error loading instance");
        }
    }
}
