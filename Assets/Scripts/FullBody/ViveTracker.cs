using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class ViveTracker : MonoBehaviour
{
    private static HashSet<uint> eIndices;

	// Use this for initialization
	void Start () {
        eIndices = new HashSet<uint>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (GetComponent<SteamVR_TrackedObject>().index == SteamVR_TrackedObject.EIndex.None)
        {
            uint index = 0;
            var error = ETrackedPropertyError.TrackedProp_Success;
            for (uint i = 0; i < 16; i++)
            {
                var result = new System.Text.StringBuilder((int)64);
                OpenVR.System.GetStringTrackedDeviceProperty(i, ETrackedDeviceProperty.Prop_RenderModelName_String, result, 64, ref error);
                
                if (result.ToString().Contains("vr_tracker_vive_1_0") && !eIndices.Contains(i))
                {
                    Debug.Log(result.ToString());
                    Debug.Log(i);
                    index = i;
                    GetComponent<SteamVR_TrackedObject>().index = (SteamVR_TrackedObject.EIndex)index;
                    eIndices.Add(index);
                    break;
                }
            }
            
        }
	}
}
