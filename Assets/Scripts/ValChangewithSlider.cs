using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ValChangewithSlider : MonoBehaviour {

    public Slider sliderObj;

	// Use this for initialization
	void Awake () {
        //displayTextObj = GameObject.Find("IntensityVal").GetComponent<Text>();
        //if (displayTextObj) displayTextObj.text = GetComponent<Slider>().value.ToString();
	}

    // Update is called once per frame
    public void Update () {
        if (sliderObj) GetComponent<Text>().text = sliderObj.value.ToString();
    }
}
