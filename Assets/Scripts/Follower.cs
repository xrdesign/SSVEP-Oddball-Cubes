using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follower : MonoBehaviour
{
    public Transform t;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        transform.position = t.position;
        transform.rotation = t.rotation;
    }

    void LateUpdate()
    {
        transform.position = t.position;
        transform.rotation = t.rotation;
    }
}
