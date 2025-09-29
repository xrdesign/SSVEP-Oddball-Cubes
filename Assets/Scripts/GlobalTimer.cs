using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalTimer : MonoBehaviour
{
    public float currentTime = -1.0f;

    // Start is called before the first frame update
    void Awake()
    {
        currentTime = Time.fixedTime;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        currentTime = Time.fixedTime;
    }
}
