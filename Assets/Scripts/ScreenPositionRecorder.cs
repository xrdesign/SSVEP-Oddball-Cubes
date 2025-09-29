using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.UI;
using System.IO;
using System;
using LSL;

public class ScreenPositionRecorder : MonoBehaviour
{
    public static ScreenPositionRecorder instance;

    void OnEnable()
    {
        if (instance != null)
        {
            Destroy(this);
            return;
        }
        instance = this;
    }

    public Camera cam;
    public Transform target;
    public bool start = false;
    public bool running = false;
    public Image startImage;
    StreamWriter output;
    float time = 0;
    string filename;

    public liblsl.StreamOutlet markerStream;
    public liblsl.StreamOutlet screenPositionStream;

    void Start()
    {
        startImage.enabled = true;
        string filename = String.Format("{1}_{0:MMddyyyy-HHmmss}{2}", DateTime.Now, "EyetrackingScreenPosition", ".csv");
        output = new StreamWriter(
            Path.Combine(@"C:\EscapeRoomData", filename)); //LogRecorder.logDir
        output.WriteLine("leftX,leftY,leftPupilDiameter,rightX,rightY," +
                         "rightPupilDiameter,timestamp");
        running = true;

        liblsl.StreamInfo inf = new liblsl.StreamInfo("PLREventMarker", "Markers", 1, 0, liblsl.channel_format_t.cf_string, "giu4569");
        markerStream = new liblsl.StreamOutlet(inf);

        liblsl.StreamInfo inf2 = new liblsl.StreamInfo("ScreenPosition", "Gaze", 7, 90, liblsl.channel_format_t.cf_float32, "gaze");
        screenPositionStream = new liblsl.StreamOutlet(inf2);
    }
    void LateUpdate()
    {
        if (start)
        {
            start = false;
            startImage.enabled = false;
            // MetroManager.SendEvent("Event: VideoSyncedThisFrame");
            // LogRecorder.SendEvent(0, new SyncGamesEvent());
            string[] tempSample = { "start" };
            markerStream.push_sample(tempSample);
        }
        if (running)
        {
            time += Time.deltaTime;
            var s = target.position;

            Matrix4x4 lpm =
                cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Left);
            Matrix4x4 wtl = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Left);
            Vector4 lTemp = wtl * new Vector4(s.x, s.y, s.z, 1);
            lTemp = lpm * lTemp;
            Vector2 lPos = new Vector2(lTemp.x, lTemp.y) / lTemp.z;
            lPos = lPos * .5f;
            lPos += Vector2.one * .5f;

            Matrix4x4 wtr = cam.GetStereoViewMatrix(Camera.StereoscopicEye.Right);
            Matrix4x4 rpm =
                cam.GetStereoProjectionMatrix(Camera.StereoscopicEye.Right);
            Vector4 rTemp = wtr * new Vector4(s.x, s.y, s.z, 1);
            rTemp = rpm * rTemp;
            Vector2 rPos = new Vector2(rTemp.x, rTemp.y) / rTemp.z;
            rPos = rPos * .5f;
            rPos += Vector2.one * .5f;
            output.WriteLine(
                $"{lPos.x},{lPos.y},{ProEyeGazeVST.leftDiameter},{rPos.x},{rPos.y},{ProEyeGazeVST.rightDiameter},{time}");
            float[] tempSample = { lPos.x, lPos.y, ProEyeGazeVST.leftDiameter, rPos.x, rPos.y, ProEyeGazeVST.rightDiameter, time };
            screenPositionStream.push_sample(tempSample);
        }
    }
}
