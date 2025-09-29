using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirrorObjectGenerator : MonoBehaviour
{
    public GameObject mirrorDirection;
    public GameObject avatar;
    public List<Transform> ts;
    public float distance;
    
    private List<Transform> mirrorTs;

	// Use this for initialization
	void Start () {
        mirrorTs = new List<Transform>();

        for (int i = 0; i < ts.Count; i++)
        {
            mirrorTs.Add(GameObject.CreatePrimitive(PrimitiveType.Sphere).transform);
            //mirrorTs[i].localScale = new Vector3(1, 1, -1);
        }
    }
	
	// Update is called once per frame
	void FixedUpdate () {
        for (int i = 0; i < ts.Count; i++)
        {
            mirrorTs[i].position = ts[i].position + Vector3.Scale(mirrorDirection.transform.forward,new Vector3(distance, 0, distance));
        }
	}

    public void EnableMirror(bool flag = true)
    {
        foreach (var t in mirrorTs)
        {
            t.gameObject.SetActive(flag);
        }
    }
}
