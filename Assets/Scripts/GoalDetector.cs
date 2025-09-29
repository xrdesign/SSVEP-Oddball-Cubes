using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalDetector : MonoBehaviour
{
    public ItemSpawner itemSpawner;
    public LSLStreamer lslStreamer;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // When trigger enter, check if the object is the target
    void OnTriggerEnter(Collider other)
    {
        // TODO send require message through LSL
        
        if (itemSpawner.target == other.gameObject)
        {
            lslStreamer.SendEvent("Received the correct item. Trial ends.");
            // One trial finished, call itemSpawner to spawn a new trial
            itemSpawner.NextTrial();
        }

        lslStreamer.SendEvent("Received the wrong item: " + other.name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
