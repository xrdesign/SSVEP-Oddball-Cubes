using aGlassDKII;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;
using LSL;
using UnityEngine.UI;
using ViveSR.anipal.Eye;

public class GameGazeNew : MonoBehaviour
{
    int count = 0;
    private StreamWriter _writer;
    private liblsl.StreamOutlet markerStream;
    Vector2 pupilPos_L;
    Vector2 pupilPos_R;

    // Use this for initialization
    void Start()
    {
        if (!SRanipal_Eye_Framework.Instance.EnableEye)
        {
            enabled = false;
            return;
        }

        print(aGlass.Instance.aGlassStart());
        string filename = String.Format("{1}_{0:MMddyyyy-HHmmss}{2}", DateTime.Now, "ProEyeData", ".txt");
        string path = Path.Combine(@"C:\EscapeRoomData", filename);
        _writer = File.CreateText(path);
        _writer.Write("\n\n=============== Game started ================\n\n");

        liblsl.StreamInfo inf =
            new liblsl.StreamInfo("ProEyeGaze", "Gaze", 4, 90, liblsl.channel_format_t.cf_float32, "sddsfsdf");
        markerStream = new liblsl.StreamOutlet(inf);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (SceneManager.GetActiveScene().buildIndex == 0 ||
                                                 SceneManager.GetActiveScene().buildIndex == 1))
        {
            Application.Quit();
        }

        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;
        if (SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.WORKING &&
            SRanipal_Eye_Framework.Status != SRanipal_Eye_Framework.FrameworkStatus.NOT_SUPPORT) return;

        if (SRanipal_Eye.GetPupilPosition(EyeIndex.LEFT, out pupilPos_L) &&
            SRanipal_Eye.GetPupilPosition(EyeIndex.RIGHT, out pupilPos_R))
        {
            _writer.WriteLine(String.Format("{0:HH:mm:ss.fff}", DateTime.Now) + " - " + Time.time.ToString() +
                              ": EyeL " + pupilPos_L + ", EyeR: " + pupilPos_R);
            float[] tempSample = {pupilPos_L.x, pupilPos_L.y, pupilPos_R.x, pupilPos_R.y,};
            markerStream.push_sample(tempSample);
        }
        else
        {
            Debug.Log("GetPupilPosition Failed");
        }
    }

    public Vector2 getCurrLeft()
    {
        return pupilPos_L;
    }

    public Vector2 getCurrRight()
    {
        return pupilPos_R;
    }

    void OnDestroy()
    {
        _writer.Close();
    }

    public void GetPos(GameObject c, ref Vector2 cx, ref Vector2 cy)
    {
        Vector3 GazeOriginCombinedLocal, GazeDirectionCombinedLocal;
        if (SRanipal_Eye.GetPupilPosition(EyeIndex.LEFT, out pupilPos_L) &&
            SRanipal_Eye.GetPupilPosition(EyeIndex.RIGHT, out pupilPos_R) &&
            SRanipal_Eye.GetGazeRay(GazeIndex.COMBINE, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal) &&
            SRanipal_Eye.GetGazeRay(GazeIndex.LEFT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal) &&
            SRanipal_Eye.GetGazeRay(GazeIndex.RIGHT, out GazeOriginCombinedLocal, out GazeDirectionCombinedLocal))
        {
            Vector3 GazeDirectionCombined = Camera.main.transform.TransformDirection(GazeDirectionCombinedLocal);
            Vector3 camPos = Camera.main.transform.position - Camera.main.transform.up * 0.05f;
            c.transform.SetPositionAndRotation(camPos, Quaternion.identity);
            c.transform.LookAt(Camera.main.transform.position + GazeDirectionCombined * 25);

            count = 0;
            if (!c.activeSelf)
            {
                c.SetActive(true);
            }

            cx = pupilPos_L;
            cy = pupilPos_R;
        }
        else
        {
            count++;
            if (count > 10 && c.activeSelf)
            {
                c.SetActive(false);
            }
        }
    }
}