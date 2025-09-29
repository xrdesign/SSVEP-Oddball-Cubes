using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlCube : MonoBehaviour
{
    bool flag;
    Flicky flickyController;
    // Start is called before the first frame update
    void Start()
    {
        flickyController = GetComponent<Flicky>();
        //flickyController.Initialize();
        flickyController.SetMainColor(Color.black);
        flickyController.SetBlinkColor(Color.white);
        flag = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (flag == false) {
            flickyController.SetFrequency(10);
            flag = true;
        }
    }
}
