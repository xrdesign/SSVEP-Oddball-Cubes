using aGlassDKII;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.IO;
using System;

using UnityEngine.UI;

public class GameGaze : MonoBehaviour
{
    int count = 0;
    private StreamWriter _writer;
    float aGlassX = 0;
    float aGlassY = 0;

    // Use this for initialization
    void Start()
    {
        print(aGlass.Instance.aGlassStart());
        string filename = String.Format("{1}_{0:MMddyyyy-HHmmss}{2}", DateTime.Now, "aGlassData", ".txt");
        string path = Path.Combine(@"C:\EscapeRoomData", filename);
        _writer = File.CreateText(path);
        _writer.Write("\n\n=============== Game started ================\n\n");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && (SceneManager.GetActiveScene().buildIndex == 0 || SceneManager.GetActiveScene().buildIndex == 1))
        {
            Application.Quit();
        }
        if (aGlass.Instance.GetEyeValid())
        {

            // print(Time.time.ToString() + " -- Eye X: " + aGlass.Instance.GetGazePoint().x + "  Y: " + aGlass.Instance.GetGazePoint().y);
            aGlassX = aGlass.Instance.GetGazePoint().x;
            aGlassY = aGlass.Instance.GetGazePoint().y;
            _writer.WriteLine(String.Format("{0:HH:mm:ss.fff}", DateTime.Now) + " - " + Time.time.ToString() + " -- Eye X: " + aGlassX + "  Y: " + aGlassY);

        }
    }

    public float getCurrX ()
    {
        return aGlassX;
    }

    public float getCurrY()
    {
        return aGlassY;
    }

    void OnDestroy()
    {
        print(aGlass.Instance.aGlassStop());
        _writer.Close();
        
    }

    public void GetPos(GameObject c, ref float cx, ref float cy)
    {
        if (aGlass.Instance.GetEyeValid())
        {
            count = 0;
            if (!c.activeSelf)
            {
                c.SetActive(true);
            }
            cx = aGlass.Instance.GetGazePoint().x;
            cy = aGlass.Instance.GetGazePoint().y;
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
