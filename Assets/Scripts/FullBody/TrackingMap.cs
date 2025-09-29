using System;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;
using UnityEngine;

[System.Serializable]
public class TrackedPoint
{
    public Transform tracker;
    public Transform rigTarget;
    public Vector3 trackingPositionOffset;
    public Quaternion trackingRotationOffset;

    public void PerformAlignment()
    {
        trackingPositionOffset = tracker.InverseTransformPoint(rigTarget.position);
        
        //tracker * offset = rigTarget
        //offset = tracker.Inverse() * rigTarget;

        trackingRotationOffset = (Quaternion.Inverse(tracker.rotation) * rigTarget.rotation);
    }

    public void Update()
    {
        rigTarget.position = tracker.TransformPoint(trackingPositionOffset);
        rigTarget.rotation = tracker.rotation * trackingRotationOffset;
    }
}

public class TrackingMap : MonoBehaviour
{
    
    public IKControl ikcontrol;
    public Transform model;
    public NVRHand left;
    public NVRHand right;
    public bool startCalibration = true;
    public bool ready = false;

    public GameObject CalibrationAsset;
    
    public List<TrackedPoint> trackedPoints;
    public List<Transform> viveTrackers;

    public ProEyeGazeVST gazeStream;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (startCalibration)
        {
            if (!CalibrationAsset.activeInHierarchy)
            {
                CalibrationAsset.SetActive(true);
            }
        }

        if (startCalibration && left.HoldButtonPressed && right.HoldButtonPressed)
        {
            PerformAlignment();
        }

        if (ready)
        {
            // Update the points
            foreach (var point in trackedPoints)
            {
                point.Update();
            }

            // Rotate and move model based on spine
            Vector3 newPosition = trackedPoints[6].rigTarget.position;
            newPosition.y = model.position.y;
            model.position = newPosition;

            Vector3 newRotation = Vector3.zero;
            newRotation.y = trackedPoints[6].rigTarget.eulerAngles.y;
            model.rotation = Quaternion.Euler(newRotation);
        }
    }

    int FindSpineTracker()
    {
        // Find Spine
        float maxY = Single.MinValue;
        int maxIndex = -1;
        for (int i = 0; i < viveTrackers.Count; i++)
        {
            float currY = viveTrackers[i].position.y;
            if (currY > maxY)
            {
                maxY = currY;
                maxIndex = i;
            }
        }

        return maxIndex;
    }

    public void PerformAlignment()
    {
        // Figure out vive tracker positions
        int spineIndex = FindSpineTracker();
        if (spineIndex < 0)
        {
            Debug.Log("Fail to find Spine");
        }

        if (gazeStream)
        {
            gazeStream.chestIMU = viveTrackers[spineIndex].GetComponent<SteamVR_TrackedObject>();
        }

        //ikcontrol.spineObj = viveTrackers[spineIndex];
        trackedPoints[3].tracker = viveTrackers[spineIndex];
        trackedPoints[6].tracker = viveTrackers[spineIndex];

        // Find Left and Right tracker
        float lastDist = -1;
        int lastIndex = -1;
        for (int i = 0; i < viveTrackers.Count; i++)
        {
            if (i == spineIndex)
            {
                continue;
            }

            Transform curr = viveTrackers[i];
            if (lastIndex >= 0)
            {
                float currDist = (curr.position - left.transform.position).magnitude;
                if (currDist > lastDist)
                {
                    // curr is right tracker
                    Debug.Log("curr is right");
                    
                    trackedPoints[4].tracker = viveTrackers[lastIndex];
                    trackedPoints[5].tracker = curr;
                }
                else
                {
                    // curr is left tracker
                    Debug.Log("curr is left");
                    
                    trackedPoints[4].tracker = curr;
                    trackedPoints[5].tracker = viveTrackers[lastIndex];
                }
            }

            lastDist = (curr.position - left.transform.position).magnitude;
            lastIndex = i;
        }


        foreach (var point in trackedPoints)
        {
            point.PerformAlignment();
        }

        ready = true;
        startCalibration = false;
        //CalibrationAsset.SetActive(false);
    }
}
