using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeatColorObject : MonoBehaviour {

    private int heatNearby = 0;
    private EyeHeapmapsManager manager;
    private bool isActive = false;

	// Use this for initialization
	void Start () {
        this.gameObject.GetComponent<Renderer>().material.color = new Color(0.0f, 1.0f, 0.0f, 0.15f);
        
    }

    private void OnDestroy()
    {
        Destroy(this.gameObject);
    }

    /*void FixedUpdate()
    {
        bool tmp = manager.getActiveStatus();
        if (manager && isActive != tmp)
            isActive = tmp;
            gameObject.GetComponent<Renderer>().enabled = tmp;
    }*/

    // Update is called once per frame
    void OnTriggerEnter(Collider collider)
    {
        if (collider.gameObject.CompareTag("Heat"))
        {
            heatNearby++;
            updateColor();
        }
    }

    void updateColor()
    {
        //if (heatNearby<2)
        //{
        if (heatNearby <= 100)
            this.gameObject.GetComponent<Renderer>().material.color = new Color(heatNearby / 100.0f, - heatNearby / 100.0f + 1, 0.0f, 0.15f);
        else
            this.gameObject.GetComponent<Renderer>().material.color = new Color(1.0f, 0.0f, 0.0f, 0.1f);
        //}


    }

    public void setManager(EyeHeapmapsManager m)
    {
        manager = m;
    }

    public void setRenderActive(bool b)
    {
        gameObject.GetComponent<Renderer>().enabled = b;
    }
}
