using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RespawnController : MonoBehaviour
{
    public string respawnTriggerName = "RespawnTrigger";

    public bool toOriginalPosition = true;
    public ItemSpawner itemSpawner;

    public Vector3 initialPosition;
    public Quaternion initialRotation;

    // Start is called before the first frame update
    void Start()
    {
        // store initial position and rotation
        initialPosition = transform.position;
        initialRotation = transform.rotation;
    }

    // When trigger enter, check trigger name
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == respawnTriggerName)
        {
            if (toOriginalPosition)
            {
                // Respawn to initial
                transform.position = initialPosition;
                transform.rotation = initialRotation;
            }
            else
            {
                itemSpawner.RandomSpawnUntilNoCollision(this.gameObject);
            }

            // reset rigidbody velocity inertia 
            Rigidbody rb = GetComponent<Rigidbody>();
            if (rb)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // reset inertia 
                //rb.inertiaTensor = Vector3.zero;
                //rb.inertiaTensorRotation = Quaternion.identity;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
