using System;
using System.Collections;
using System.Collections.Generic;
using LSL;
using UnityEngine;
using UnityEngine.Analytics;

public class PLRCue : MonoBehaviour
{

    public enum PLRMode
    {
        Halo,
        Amplitude,
        Count
    }

    public string name = "";
    public float frequency = 1.0f;
    public Renderer selfRenderer = null;
    public bool isEnable = false;
    public float accumTime = 0.0f;
    public float step = 0.0f;
    public float offset = 0.0f;

    public Light halo;
    public SSVEPCueNew ssvep;

    public Color onColor = Color.white;
    public Color onColorDim = Color.white;
    public Color offColor = Color.black;
    public Color oldColor;
    public Color oldSSVEPColor;

    private Material _cloneMat;
    private bool _currState = false;

    public PLRMode mode;
    public CalibrationTaskManager manager;

    public void Toggle(bool flag)
    {
        isEnable = flag;
        if (isEnable)
        {
            if (_cloneMat)
            {
                _cloneMat.color = offColor;
                ssvep.ChangeOnColor(onColorDim);
            }
        }
        else
        {
            if (_cloneMat)
            {
                _cloneMat.color = oldColor;
                ssvep.ChangeOnColor(oldSSVEPColor);
            }
        }
        accumTime = 0.0f;
    }

    // Start is called before the first frame update
    void Start()
    {
        halo = GetComponent<Light>();
        ssvep = GetComponent<SSVEPCueNew>();
        oldColor = halo.color;
        oldSSVEPColor = ssvep.onColor;
    }

    public void ChangeMode(int mode)
    {
        this.mode = (PLRMode)mode;
        if (halo)
        {
            halo.color = offColor;
        }
        if (ssvep)
        {
            ssvep.ChangeOnColor(oldSSVEPColor);
        }
        accumTime = 0.0f;
        _currState = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isEnable)
        {
            //if (_currState)
            //{
            var currDT = Time.deltaTime;

            accumTime += currDT;
            if (accumTime - offset < 0)
            {
                return;
            }
            step = 1.0f / frequency;
            //if ((accumTime - offset) > step / 2.0f)
            //{
            // turn on
            if (!_currState)
            {
                if ((accumTime - offset) > 3 * step / 4.0f)
                {
                    switch (mode)
                    {
                        case PLRMode.Halo:
                            halo.color = onColor;
                            break;
                        case PLRMode.Amplitude:
                            ssvep.ChangeOnColor(oldSSVEPColor);
                            break;
                    }

                    _currState = true;

                    accumTime -= (3 * step / 4.0f);
                    manager.SendPLRMarker(name);

                }
            }
            else
            {
                if ((accumTime - offset) > step / 4.0f)
                {
                    switch (mode)
                    {
                        case PLRMode.Halo:
                            halo.color = offColor;
                            break;
                        case PLRMode.Amplitude:
                            ssvep.ChangeOnColor(onColorDim);
                            break;
                    }

                    _currState = false;

                    accumTime -= (step / 4.0f);
                }
            }

            //}
            //}
            //else
            //{
            // turn off
            //    _cloneMat.color = offColor;
            //    _currState = false;
            //}
        }
        else
        {

            ssvep.ChangeOnColor(oldSSVEPColor);
            halo.color = oldColor;
            accumTime = 0.0f;

            _currState = false;
        }

    }
}
