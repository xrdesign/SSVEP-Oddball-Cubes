using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using NewtonVR;

public class SpinnerUnlocker : MonoBehaviour
{
    public NVRDigitSpinner Spinner1;
    public NVRDigitSpinner Spinner2;
    public NVRDigitSpinner Spinner3;

    public string unlockPW = "000";
    public string currentPW;

    public AudioClip unlockSound;

    public AudioSource source;
   

    private bool didSound = false;

    private void Awake()
    {
        currentPW = "2333";
    }

    private void Update()
    {
        currentPW = Spinner1.GetLetter() + Spinner2.GetLetter() + Spinner3.GetLetter();
        if (currentPW == unlockPW && !didSound)
        {
            
            GetComponent<NVRInteractableItem>().CanAttach = true;
            source.Play();
            Spinner1.CanAttach = false;
            Spinner2.CanAttach = false;
            Spinner3.CanAttach = false;
            GetComponent<GradualMover>().UnlockAfter(2.0f);
            didSound = true;
        }

        
        
    }
}