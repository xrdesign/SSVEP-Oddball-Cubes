using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{

    public AudioSource clockSound;

    // Play the success/fail sound for bullet room
    public void ChangeClock(AudioClip music)
    {
        clockSound.Pause();
        clockSound.clip = music;
        clockSound.Play();
        clockSound.loop = false;
        //clockSound.Stop();
    }

    // Play the ticking sound for bullet room
    public void ChangeClockLoop(AudioClip music)
    {
        clockSound.Pause();
        clockSound.clip = music;
        clockSound.Play();
        clockSound.loop = true;
    }
}
