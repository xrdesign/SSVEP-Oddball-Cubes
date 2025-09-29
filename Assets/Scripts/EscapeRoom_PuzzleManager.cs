using NewtonVR;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using LSL;

public class EscapeRoom_PuzzleManager : MonoBehaviour
{

    private bool solved = false;

    public bool[] unlockBool = { false, false, false };

    public AudioClip unlockSound;
    public AudioClip lockSound;
    public AudioSource source;
    public NVRInteractableItem objToUnlock;

    private liblsl.StreamOutlet markerStream;

    // Use this for initialization
    void Start()
    {

        liblsl.StreamInfo inf = new liblsl.StreamInfo("TestMarker", "Markers", 1, 0, liblsl.channel_format_t.cf_string, "giu4569");
        markerStream = new liblsl.StreamOutlet(inf);
    }

    /*IEnumerator WriteContinouslyMarkerEachSecond()
    {
        while (true)
        {
            // an example for demonstrating the usage of marker stream
            var currentMarker = unlockBool();
            
            markerStream.Write(currentMarker);
            yield return new WaitForSecondsRealtime(1f);
        }
    }*/

    // Update is called once per frame
    /*void Update()
    {

    }*/

    public void SendToggleHeatmapEvent(bool enable)
    {
        string[] tempSample;
        if (enable)
        {
            tempSample = new string[] { "Enable Heatmap" };
        }
        else
        {
            tempSample = new string[] { "Disable Heatmap" };
        }
        markerStream.push_sample(tempSample);
    }

    public void SendButton(int index)
    {
        /*
         *  1 4
         *  5 2
         *  3 6
         */
        string markerString = "Button " + index + " is pressed";
        Debug.Log(markerString);
        string[] tempSample = { markerString };
        markerStream.push_sample(tempSample);
    }

    public void unlock(int i)
    {
        print("sucess " + i);
        unlockBool[i] = true;
        checkUnlockAll();

        string[] tempSample = { "Correct disk " + i };
        markerStream.push_sample(tempSample);
    }

    public void lockback(int i)
    {
        print("lock " + i);
        unlockBool[i] = false;

        string[] tempSample = { "Wrong disk " + i };
        markerStream.push_sample(tempSample);
    }

    void checkUnlockAll()
    {
        bool allUnlock = true;
        foreach (bool b in unlockBool)
        {
            allUnlock = allUnlock && b;
        }

        if (objToUnlock != null && allUnlock && !solved)
        {
            print("Unlock All");

            objToUnlock.CanAttach = true;
            objToUnlock.GetComponent<GradualMover>().Unlock();
            source.PlayOneShot(unlockSound, 1.0f);
            solved = true;

            string[] tempSample = { "(1) Unlock All Disk"};
            markerStream.push_sample(tempSample);
        }
        else {
            print("Unlock All: " + allUnlock + "  solved?: " + solved );
        }
    }
}
