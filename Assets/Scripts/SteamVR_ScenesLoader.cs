using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SteamVR_ScenesLoader : MonoBehaviour {

    public int currLevel = 0;

    public string[] sceneName = new string[4] { "Room_1_v3", "Room_2_v2", "Room_3_v1", "Tutorial" };

    public Dropdown dropdownObj;

    public void LoadingScene(int index)
    {
        SteamVR_LoadLevel.Begin(sceneName[index], false, 1.0f);
    }

    public void LoadingSceneFromDropdown()
    {
        if (dropdownObj)
            SteamVR_LoadLevel.Begin(sceneName[dropdownObj.value], false, 1.0f);
    }
}
