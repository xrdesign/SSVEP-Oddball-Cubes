using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EyeHeapmapsManager : MonoBehaviour {


    public HeatColorObject CustomizedHeatObject;

    private List<HeatColorObject> objs;

    public Transform GazeTransform;

    public Transform ParentOfHeat;

    private RaycastHit hit;

    private int ignoreLayerMask = 10;

    public bool heatActive = true;

    public float timer = 0.2f;

    private float secondsCount = 0.0f;

    // Use this for initialization
    void Start () {
        objs = new List<HeatColorObject>();
        if (!CustomizedHeatObject)
        {
            CustomizedHeatObject = new HeatColorObject();
        }
		
	}
	
	// Update is called once per frame
	void Update () {
        secondsCount += Time.deltaTime;
        if (GazeTransform && secondsCount > timer)
        {
            
            if (Physics.Raycast(GazeTransform.position, GazeTransform.forward, out hit, 20, ~(1 << 10)))
            {
                //if (!hit.collider.gameObject.CompareTag("Heat"))
                HeatColorObject ins  = Instantiate(CustomizedHeatObject, hit.point, Quaternion.identity);
                
                if (ins)
                {
                    ins.transform.parent = (ParentOfHeat ? ParentOfHeat : gameObject.transform);
                    ins.setManager(this);
                    objs.Add(ins);
                }
                
            }
            //Debug.Log("set heat");
            secondsCount = 0.0f;
        }
    }

    public bool getActiveStatus()
    {
        return heatActive;
    }

    public void setHeatActive(bool b)
    {
        heatActive = b;
    }

    public void clearHeatmaps()
    {
        foreach (HeatColorObject obj in objs)
        {
            Destroy(obj);
        }
    }

    public void setFrequency(string s)
    {
        float timeFloat = 0.0f;
        if (float.TryParse(s, out timeFloat))
        {
            timer = timeFloat;
        } else
        {
            Debug.Log("String - " + s + " - is not float");
        }
    }
}
