using System.Collections;
using System.Collections.Generic;
using LSL;
using UnityEngine;

public class LSLStreamer : MonoBehaviour
{
    public liblsl.StreamOutlet markerStream;

    public liblsl.StreamOutlet timerStream;

    // Start is called before the first frame update
    void Start()
    {
        liblsl.StreamInfo inf = new liblsl.StreamInfo("EventMarker", "Markers", 1, 0, liblsl.channel_format_t.cf_string,
            "giu4569");
        markerStream = new liblsl.StreamOutlet(inf);

        liblsl.StreamInfo inf2 = new liblsl.StreamInfo("TimerStream", "Markers", 1, 0, liblsl.channel_format_t.cf_float32,
            "giu4569");
        timerStream = new liblsl.StreamOutlet(inf2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SendEvent(string eventString)
    {
        string[] tempSample;
        tempSample = new string[] { eventString };
        markerStream.push_sample(tempSample);
    }

    public void SendTimer(float currentTime)
    {
        timerStream.push_sample(new float[] { currentTime });
    }
}
