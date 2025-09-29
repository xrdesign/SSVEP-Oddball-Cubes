using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuPopoutButton : MonoBehaviour {

    //public GameObject Gmenue;
    public GameObject btnObj;
    public GameObject menuObj;
    public Sprite expan;
    public Sprite back;
    Button btn;
    bool isshow = false;
    // Use this for initialization
    void Start()
    {
        menuObj.SetActive(isshow);
        btn = btnObj.GetComponent<Button>();
        btn.onClick.AddListener(delegate ()
        {
            isshow = !isshow;
            menuObj.SetActive(isshow);
            if (isshow)
            {
                btn.GetComponent<Image>().sprite = expan;
            }
            else
            {
                btn.GetComponent<Image>().sprite = back;
            }
        });
    }

    // Update is called once per frame
    void Update () {
		
	}
}