using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class dropdownValChangeTrigger : MonoBehaviour {

    public Dropdown dropObj;
    public SpectatorController spetatorCtr;

    // Use this for initialization
    void Start () {
		if (dropObj == null)
        {
            dropObj = this.GetComponent<Dropdown>();
        }
        //Add listener for when the value of the Dropdown changes, to take action
        dropObj.onValueChanged.AddListener(delegate {
            DropdownValueChanged(dropObj);
        });
    }

    //Ouput the new value of the Dropdown into Text
    void DropdownValueChanged(Dropdown change)
    {
        if (spetatorCtr != null && change.options.Count <= spetatorCtr.AttachmentPoints.Count)
            spetatorCtr.SwitchCamera(change.value);
    }

    public void nextValue()
    {
        if (dropObj.value < dropObj.options.Count-1)
            dropObj.value++;
        else
            dropObj.value = 0;
    }
}
