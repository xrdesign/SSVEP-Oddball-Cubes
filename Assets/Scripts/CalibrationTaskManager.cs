using System.Collections;
using System.Collections.Generic;
using System.Linq;
using LSL;
using NewtonVR;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Utility extensions used by the calibration workflow.
/// </summary>
public static class ExtensionForCalibration
{
    public static void ShuffleList<T>(this IList<T> list)
    {
        System.Random random = new System.Random();
        int n = list.Count;

        for (int i = list.Count - 1; i > 1; i--)
        {
            int rnd = random.Next(i + 1);

            T value = list[rnd];
            list[rnd] = list[i];
            list[i] = value;
        }
    }
}
/// <summary>
/// Coordinates the calibration paradigm (ERP/SSVEP/PLR) by sequencing cues,
/// logging LSL markers, and reacting to controller input.
/// </summary>
public class CalibrationTaskManager : MonoBehaviour
{

    public enum State
    {
        Stop,
        Center,
        ShowTarget,
        SecondCube, // only relavant in two_step mode
    }

    public enum BaselineState
    {
        Stop,
        Rest,
        ShowSSVEPTarget,
        ShowPLRTarget1,
        ShowPLRTarget2,
        ShowPLRTarget3,
        ShowPLRTarget4,
    }

    public enum Mode
    {
        original = 0,
        two_step
    }

    // --- Experiment toggles ---
    public bool useNewSSVEP = false;

    // --- Mode selectors ---
    public Dropdown dropdown_mode;

    public Dropdown dropdown_plr_mode;

    [Tooltip("Note: To get even number of trials on each freq, the actual trial number for the **ssvep** paradigm will be trial_count * 4")]
    public int trial_count = 120;
    public float odd = 0.2f;

    // --- Timing ---
    public float duration = 1.0f;

    public float rest_duration = 1.5f;
    private float custom_rest_duration = -1.0f;

    public float starting_time = 0;
    public float dist_threshold = 0.4f;

    public bool started = false;
    public bool resetted = true;

    // --- Scene references ---
    public GameObject cube_parent;
    public GameObject head;

    // --- Ring configuration ---
    public bool r1Enable = true;
    public List<GameObject> r1Cubes = new List<GameObject>();
    public bool r2Enable = true;
    public List<GameObject> r2Cubes = new List<GameObject>();
    public bool r3Enable = true;
    public List<GameObject> r3Cubes = new List<GameObject>();

    public List<GameObject> baseCubes = new List<GameObject>();

    public Mode mode = Mode.original;

    // --- Visual cues ---
    public GameObject targetSign;
    public GameObject deviantSign;
    public GameObject leftQuadSign;
    public GameObject rightQuadSign;

    public List<GameObject> leftQuadSigns = new List<GameObject>();
    public List<GameObject> rightQuadSigns = new List<GameObject>();

    public GameObject arrow;
    public GameObject back_arrow_obj;

    // only relavant if two_step mode
    public GameObject secondCube;
    public float distance = 0.2f;

    private Random r_index;
    private Random r_odd;

    public liblsl.StreamOutlet markerStream;
    private liblsl.StreamOutlet controllerStream;

    private State currentState = State.Stop;
    private BaselineState currentBaselineState = BaselineState.Stop;
    private bool switching = false;
    public float nextSwitchTime = 0;
    private int ring_index = 0;
    private int trial = 0;

    private int cube_index = 0;
    private bool isTarget = false;

    private Vector3 lastPosition;

    public bool paused = false;
    public bool ssvep = false;
    public bool plr = false;
    public bool auto = true;
    public bool shape = true;
    public bool back_arrow = true;

    public PLRCue.PLRMode plrmode = PLRCue.PLRMode.Halo;

    public GameObject lighter;
    public NVRHand left;
    public NVRHand right;

    private List<List<GameObject>> rings = new List<List<GameObject>>();

    private bool controllerPressed = false;
    private List<int> indices_table = new List<int>();
    private List<bool> left_table = new List<bool>(4);
    public List<Color> possibleColorList = new List<Color>();

    private bool outerFirst = true;

    public GameObject OnScreenUI;

    #region 2D

    public Transform target_center;
    public Transform vr_camera;
    public Transform dummy_camera;
    public Follower display;

    public float dist_2d = 0.5f;

    public bool is_2d = false;
    public Text text_2d;
    #endregion

    /// <summary>
    /// Convenience wrapper for sending single-value LSL markers.
    /// </summary>
    private void PushMarker(string message, bool alsoLog = false)
    {
        if (markerStream == null)
        {
            return;
        }

        if (alsoLog)
        {
            Debug.Log(message);
        }

        markerStream.push_sample(new[] { message });
    }

    private void PushTrialMarker(int ring, int trialIndex, int cubeIndex, int frequency, Vector3 position, bool isTargetCube)
    {
        string label = isTargetCube ? "Cube index(L)" : "Cube index(R)";
        string message = $"Ring {ring}; Trial {trialIndex}; {label}: {cubeIndex}; Frequency: {frequency}; Position: {position.ToString("G4")}";
        PushMarker(message, alsoLog: true);
    }

    private void PushTrialMarker(int ring, int trialIndex, int cubeIndex, Vector3 position, bool isTargetCube)
    {
        string label = isTargetCube ? "Cube index(L)" : "Cube index(R)";
        string message = $"Ring {ring}; Trial {trialIndex}; {label}: {cubeIndex}; Position: {position.ToString("G4")}";
        PushMarker(message, alsoLog: true);
    }

    private void PushSecondCubeMarker(int ring, int trialIndex, Vector3 position)
    {
        string message = $"Ring {ring}; Trial {trialIndex}; Second cube: {position.ToString("G4")}";
        PushMarker(message, alsoLog: true);
    }

    /// <summary>
    /// Toggles the on-screen UI when Ctrl+H is pressed.
    /// </summary>
    private void HandleUiToggleShortcut()
    {
        if (Input.GetKeyDown(KeyCode.H) && (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)))
        {
            OnScreenUI.SetActive(!OnScreenUI.activeSelf);
        }
    }

    /// <summary>
    /// Samples controller positions and emits button/trigger markers.
    /// </summary>
    private float[] CaptureControllerPose()
    {
        float[] controllerPos = new float[6];

        if (left)
        {
            controllerPos[0] = left.CurrentPosition.x;
            controllerPos[1] = left.CurrentPosition.y;
            controllerPos[2] = left.CurrentPosition.z;

            if (left.HoldButtonDown)
            {
                PushMarker("Left Trigger is holded");
                Debug.Log("left holded");
            }
        }

        if (right)
        {
            controllerPos[3] = right.CurrentPosition.x;
            controllerPos[4] = right.CurrentPosition.y;
            controllerPos[5] = right.CurrentPosition.z;

            if (right.HoldButtonDown)
            {
                PushMarker("Right Trigger is holded");
                Debug.Log("right holded");
            }
        }

        bool leftUse = left != null && left.UseButtonDown;
        bool rightUse = right != null && right.UseButtonDown;
        if (leftUse || rightUse)
        {
            PushMarker("A button is holded");
            Debug.Log("A holded");
            if (currentState == State.Center)
            {
                controllerPressed = true;
            }
        }

        return controllerPos;
    }

    // Start is called before the first frame update
    void Start()
    {
        liblsl.StreamInfo inf = new liblsl.StreamInfo("EventMarker", "Markers", 1, 0, liblsl.channel_format_t.cf_string, "giu4569");
        markerStream = new liblsl.StreamOutlet(inf);

        // controller position 
        liblsl.StreamInfo inf2 = new liblsl.StreamInfo("Controller", "MoCap", 6, 0, liblsl.channel_format_t.cf_float32, "ViveController");
        controllerStream = new liblsl.StreamOutlet(inf2);

        r_index = new Random();
        r_odd = new Random();
        targetSign.SetActive(false);
        deviantSign.SetActive(false);
        leftQuadSign.SetActive(false);
        rightQuadSign.SetActive(false);
        arrow.SetActive(false);

        //r1Cubes.ForEach(x => x.SetActive(false));
        //r2Cubes.ForEach(x => x.SetActive(false));
        //r3Cubes.ForEach(x => x.SetActive(false));
        secondCube.SetActive(false);
        if (r1Enable)
        {
            rings.Add(r1Cubes);
        }

        if (r2Enable)
        {
            rings.Add(r2Cubes);
        }

        if (r3Enable)
        {
            rings.Add(r3Cubes);
        }

        rings.Add(baseCubes);

        for (int i = 0; i < rings.Count; i++)
        {
            for (int j = 0; j < rings[i].Count; j++)
            {
                GameObject go = rings[i][j];
                if (useNewSSVEP)
                {
                    go.GetComponent<SSVEPCueNew>().frequency = 8 + j;
                }
                else
                {
                    go.GetComponent<SSVEPCue>().frequency = 8 + j;
                }

                if (go.GetComponent<PLRCue>())
                {
                    go.GetComponent<PLRCue>().offset = j * 0.25f;
                }
            }
        }

        SetMode();
        Reset();
    }

    public void SetFrequency(int index)
    {
        for (int i = 0; i < rings.Count; i++)
        {
            for (int j = index; j < index + 4; j++)
            {
                GameObject go = rings[i][j % 4];

                // index == 0 => 8, 9, 10, 11
                // index == 1 => 11, 8, 9, 10
                // index == 2 => 10, 11, 8, 9
                // index == 3 => 9, 10, 11, 8
                // rotate counterclockwise

                int frequency = 8 + (j - index);
                //Debug.Log("frequency: " + frequency);

                if (useNewSSVEP)
                {

                    go.GetComponent<SSVEPCueNew>().frequency = frequency;
                }
                else
                {
                    go.GetComponent<SSVEPCue>().frequency = frequency;
                }
            }
        }
    }

    public static float DistanceToLine(Ray ray, Vector3 point)
    {
        return Vector3.Cross(ray.direction, point - ray.origin).magnitude;
    }

    public void StartExperiment()
    {
        SetMode();

        started = true;
        switching = true;
        paused = false;
        currentState = State.Stop;

        switch (dropdown_mode.value)
        {
            case 0:
                ring_index = 0;
                break;
            case 1:
                ring_index = 2;
                break;
            case 2:
                if (outerFirst)
                {
                    ring_index = 2;
                }
                else
                {
                    ring_index = 1;
                }

                break;
            case 3:
                ring_index = 2;
                break;
        }

        //ring_index = 0;
        trial = 0;

        // set cube to head level
        if (!is_2d)
        {
            Vector3 new_pos = cube_parent.transform.position;
            new_pos.y = head.transform.position.y;
            new_pos.x = head.transform.position.x;
            cube_parent.transform.position = new_pos;
        }

        Reset();

        starting_time = Time.time;
        PushMarker("Calibration Start");

        ScreenPositionRecorder.instance.start = true;
        // set to OnScreenUI to false
        if (OnScreenUI)
        {
            OnScreenUI.SetActive(false);
        }
    }

    public void SetMode()
    {
        switch (dropdown_mode.value)
        {
            case 0:
                PushMarker("ERP enabled");
                ring_index = 0;
                break;
            case 1:
                PushMarker("SSVEP 1 enabled");
                ring_index = 2;
                break;
            case 2:
                PushMarker("SSVEP 2 enabled");
                ring_index = outerFirst ? 2 : 1;
                indices_table.Clear();
                for (int i = 0; i < 4; i++)
                {
                    for (int j = 0; j < trial_count; j++)
                    {
                        indices_table.Add(i);
                    }
                }
                indices_table.ShuffleList();
                Debug.Log(string.Join(", ", indices_table));
                break;
            case 3:
                PushMarker("Baseline enabled");
                ring_index = 2;
                break;
            case 4:
                PushMarker("SSVEP 4 enabled");
                break;
        }
        Debug.Log("Change mode to:" + dropdown_mode.value);
        started = false;
        resetted = false;
    }

    public void SetPause(bool pause)
    {
        paused = pause;
        PushMarker(paused ? "Paused" : "Resumed");
    }

    public void SetAuto(bool auto)
    {
        this.auto = auto;
        PushMarker(this.auto ? "Set auto proceed to next trial" : "Manually proceed to next trial");
    }

    public void SetShape(bool shape)
    {
        this.shape = shape;
        if (this.shape)
        {
            RandomRotationAndColor(true, true, possibleColorList);
            PushMarker("Enable shape");
        }
        else
        {
            leftQuadSigns.ForEach(x => x.SetActive(false));
            rightQuadSigns.ForEach(x => x.SetActive(false));
            PushMarker("Disable shape");
        }
    }

    public void SetBackArrow(bool back_arrow)
    {
        this.back_arrow = back_arrow;
        if (this.back_arrow)
        {
            PushMarker("Enable back_arrow");
        }
        else
        {
            back_arrow_obj.SetActive(false);
            PushMarker("Disable back_arrow");
        }
    }

    public void SetPLR(bool enable)
    {
        plr = enable;
        PushMarker(plr ? "plr enabled" : "plr disabled");
        rings.ForEach(x => x.ForEach(y => y.GetComponent<PLRCue>().Toggle(plr)));
    }

    public void SetPLRMode()
    {
        string message = dropdown_plr_mode.value == 0 ? "PLR Halo enabled" : "PLR amplitude enabled";
        PushMarker(message);
        rings.ForEach(x => x.ForEach(y => y.GetComponent<PLRCue>().ChangeMode(dropdown_plr_mode.value)));
        Debug.Log("Change mode to:" + dropdown_plr_mode.value);
    }

    public void SetSSVEP(bool ssvep)
    {
        this.ssvep = ssvep;
        PushMarker(this.ssvep ? "SSVEP enabled" : "SSVEP disabled");
        started = false;
    }

    public void SetOrder(bool outerFirst)
    {
        this.outerFirst = outerFirst;
        if (this.outerFirst)
        {
            ring_index = 2;
            PushMarker("Outer block plays first.");
        }
        else
        {
            ring_index = 1;
            PushMarker("Inner block plays first.");
        }
        Reset();
        started = false;
    }

    public void SendPLRMarker(string name)
    {
        string message = name + " on.";
        PushMarker(message, alsoLog: true);
    }

    // Update is called once per frame
    void Update()
    {
        HandleUiToggleShortcut();

        float[] controllerPos = CaptureControllerPose();
        controllerStream.push_sample(controllerPos);

        // Logic
        if (started)
        {
            resetted = false;
            if (paused)
            {
                nextSwitchTime += Time.deltaTime;
                return;
            }

            switch (mode)
            {
                case Mode.original:
                    switch (dropdown_mode.value)
                    {
                        case 0: // ERP
                            switch (currentState)
                            {
                                case State.Stop:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                                case State.Center:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        if (trial >= trial_count)
                                        {
                                            // Increment ring
                                            trial = 0;
                                            ring_index += 1;
                                            // Debug.Log(rings.Count);
                                        }

                                        if (ring_index >= rings.Count - 1) // ignoring baseCube
                                        {
                                            // End
                                            currentState = State.Stop;
                                            started = false;
                                        }
                                        else
                                        {
                                            currentState = State.ShowTarget;
                                        }

                                        switching = true;
                                    }

                                    break;
                                case State.ShowTarget:

                                    if (switching)
                                    {
                                        // Show stuff

                                        // Check standard or deviant
                                        float o = Random.Range(0.0f, 1.0f);
                                        if (o >= odd)
                                        {
                                            isTarget = false;
                                        }
                                        else
                                        {
                                            isTarget = true;
                                        }

                                        // Check which cube to show
                                        cube_index = Random.Range(0, rings[ring_index].Count);

                                        // Show corresponding things
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        c.SetActive(true);
                                        int freq = 0;
                                        if (useNewSSVEP)
                                        {
                                            c.GetComponent<SSVEPCueNew>().Toggle(false);
                                            freq = (int)c.GetComponent<SSVEPCueNew>().frequency;

                                        }
                                        else
                                        {
                                            c.GetComponent<SSVEPCue>().Toggle(false);
                                            freq = (int)c.GetComponent<SSVEPCue>().frequency;
                                        }

                                        // Sign
                                        if (isTarget)
                                        {
                                            targetSign.SetActive(true);
                                            // Move Sign
                                            Vector3 pos = c.transform.position;
                                            pos.z += (-0.06f);
                                            targetSign.transform.position = pos;
                                        }
                                        else
                                        {
                                            deviantSign.SetActive(true);
                                            // Move Sign
                                            Vector3 pos = c.transform.position;
                                            pos.z += (-0.06f);
                                            deviantSign.transform.position = pos;
                                        }

                                        // Send event message
                                        PushTrialMarker(ring_index, trial, cube_index, freq, c.transform.position, isTarget);

                                        nextSwitchTime = Time.time + duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        c.SetActive(false);

                                        if (isTarget)
                                        {
                                            targetSign.SetActive(false);
                                        }
                                        else
                                        {
                                            deviantSign.SetActive(false);
                                        }

                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                            }
                            break;
                        case 1: // SSVEP 1: arrow
                            switch (currentState)
                            {
                                case State.Stop:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                                case State.Center:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        if (trial >= trial_count)
                                        {
                                            // Increment ring
                                            trial = 0;
                                            //ring_index += 1;
                                        }

                                        if (ring_index >= rings.Count)
                                        {
                                            // End
                                            currentState = State.Stop;
                                            started = false;
                                        }
                                        else
                                        {
                                            currentState = State.ShowTarget;
                                        }

                                        switching = true;
                                    }

                                    break;
                                case State.ShowTarget:

                                    if (switching)
                                    {
                                        // Show stuff

                                        // Check standard or deviant
                                        float o = Random.Range(0.0f, 1.0f);
                                        if (o >= odd)
                                        {
                                            isTarget = false;
                                        }
                                        else
                                        {
                                            isTarget = true;
                                        }

                                        // Check which cube to show
                                        cube_index = Random.Range(0, rings[ring_index].Count);

                                        // Show corresponding things
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        c.SetActive(true);
                                        //c.GetComponent<SSVEPCue>().Toggle(true);
                                        int freq = 0;
                                        if (useNewSSVEP)
                                        {
                                            if (c.GetComponent<SSVEPCueNew>())
                                            {
                                                freq = (int)c.GetComponent<SSVEPCueNew>().frequency;
                                            }
                                        }
                                        else
                                        {
                                            if (c.GetComponent<SSVEPCue>())
                                            {
                                                freq = (int)c.GetComponent<SSVEPCue>().frequency;
                                            }
                                        }

                                        // set center arrow
                                        arrow.SetActive(true);
                                        GameObject ci = rings[0][cube_index];

                                        // rotation
                                        arrow.transform.eulerAngles = new Vector3(0f, 0f, cube_index * 90.0f + 180.0f);

                                        // location
                                        Vector3 arrow_pos = ci.transform.position;
                                        arrow_pos.z += (-0.07f);
                                        arrow.transform.position = arrow_pos;

                                        float angle = Random.Range(0, 360.0f);

                                        // Sign
                                        if (isTarget)
                                        {
                                            targetSign.SetActive(true);
                                            // Move Sign
                                            Vector3 pos = c.transform.position;
                                            pos.z += (-0.06f);
                                            targetSign.transform.position = pos;
                                            Vector3 euAngle = targetSign.transform.eulerAngles;
                                            euAngle.z = angle;
                                            targetSign.transform.eulerAngles = euAngle;
                                        }
                                        else
                                        {
                                            deviantSign.SetActive(true);
                                            // Move Sign
                                            Vector3 pos = c.transform.position;
                                            pos.z += (-0.06f);
                                            deviantSign.transform.position = pos;
                                            Vector3 euAngle = deviantSign.transform.eulerAngles;
                                            euAngle.z = angle;
                                            deviantSign.transform.eulerAngles = euAngle;
                                        }

                                        // Send event message
                                        PushTrialMarker(ring_index, trial, cube_index, freq, c.transform.position, isTarget);

                                        nextSwitchTime = Time.time + duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        //c.GetComponent<SSVEPCue>().Toggle(false);

                                        if (isTarget)
                                        {
                                            targetSign.SetActive(false);
                                        }
                                        else
                                        {
                                            deviantSign.SetActive(false);
                                        }

                                        arrow.SetActive(false);

                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                            }
                            break;
                        case 2: // SSVEP 2: arrow
                            switch (currentState)
                            {
                                case State.Stop:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                                case State.Center:
                                    if (switching)
                                    {

                                        if (auto)
                                        {
                                            float random_offset = Random.Range(0.0f, 0.4f);
                                            nextSwitchTime = Time.time + rest_duration + random_offset;
                                            if (shape)
                                            {
                                                RandomRotationAndColor(true, true, possibleColorList);
                                            }
                                            switching = false;
                                        }
                                        else
                                        {
                                            if (controllerPressed)
                                            {
                                                float random_offset = Random.Range(0.3f, 0.7f);
                                                nextSwitchTime = Time.time + random_offset;
                                                if (shape)
                                                {
                                                    RandomRotationAndColor(true, true, possibleColorList);
                                                }
                                                controllerPressed = false;
                                                switching = false;
                                            }
                                            else
                                            {
                                                nextSwitchTime = Time.time + 2.0f;
                                            }
                                        }
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        if (trial >= trial_count * 4)
                                        {
                                            // Increment ring
                                            trial = 0;
                                            rings[ring_index].ForEach(y => y.SetActive(false));
                                            if (outerFirst)
                                            {
                                                ring_index -= 1;
                                            }
                                            else
                                            {
                                                ring_index += 1;
                                            }

                                            if (ring_index >= 1 && ring_index <= 2)
                                            {
                                                rings[ring_index].ForEach(y => y.SetActive(true));
                                            }

                                            indices_table.ShuffleList();

                                            AlignShapeToCurrentRing();
                                        }

                                        if (ring_index < 1 || ring_index > 2)
                                        {
                                            // End
                                            currentState = State.Stop;
                                            started = false;
                                            SetMode();
                                            Reset();
                                        }
                                        else
                                        {
                                            if (back_arrow)
                                            {
                                                back_arrow_obj.SetActive(false);
                                            }
                                            currentState = State.ShowTarget;

                                            switching = true;
                                        }

                                    }

                                    break;
                                case State.ShowTarget:

                                    if (switching)
                                    {
                                        // Show stuff

                                        // Check standard or deviant
                                        //float o = Random.Range(0.0f, 1.0f);
                                        //if (o >= odd)
                                        //{
                                        //    isTarget = false;
                                        //}
                                        //else
                                        //{
                                        //    isTarget = true;
                                        //}

                                        // Check which cube to show
                                        cube_index = indices_table[trial]; //Random.Range(0, rings[ring_index].Count);

                                        // Check left or right
                                        if (leftQuadSigns[cube_index].activeInHierarchy)
                                        {
                                            isTarget = false;
                                        }
                                        else
                                        {
                                            isTarget = true;
                                        }

                                        // Show corresponding things
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        c.SetActive(true);
                                        //c.GetComponent<SSVEPCue>().Toggle(true);

                                        int freq = 0;
                                        if (useNewSSVEP)
                                        {
                                            if (c.GetComponent<SSVEPCueNew>())
                                            {
                                                freq = (int)c.GetComponent<SSVEPCueNew>().frequency;
                                            }
                                        }
                                        else
                                        {
                                            if (c.GetComponent<SSVEPCue>())
                                            {
                                                freq = (int)c.GetComponent<SSVEPCue>().frequency;
                                            }
                                        }

                                        // set center arrow
                                        arrow.SetActive(true);
                                        GameObject ci = rings[0][cube_index];

                                        // rotation
                                        arrow.transform.eulerAngles = new Vector3(0f, 0f, cube_index * 90.0f + 180.0f);

                                        // location
                                        Vector3 arrow_pos = ci.transform.position;
                                        arrow_pos.z += (-0.07f);
                                        arrow.transform.position = arrow_pos;

                                        //float angle = Random.Range(0, 360.0f);

                                        // Sign
                                        //if (isTarget)
                                        //{
                                        //    targetSign.SetActive(true);
                                        //    // Move Sign
                                        //    Vector3 pos = c.transform.position;
                                        //    pos.z += (-0.06f);
                                        //    targetSign.transform.position = pos;
                                        //    Vector3 euAngle = targetSign.transform.eulerAngles;
                                        //    euAngle.z = angle;
                                        //    targetSign.transform.eulerAngles = euAngle;
                                        //}
                                        //else
                                        //{
                                        //    deviantSign.SetActive(true);
                                        //    // Move Sign
                                        //    Vector3 pos = c.transform.position;
                                        //    pos.z += (-0.06f);
                                        //    deviantSign.transform.position = pos;
                                        //    Vector3 euAngle = deviantSign.transform.eulerAngles;
                                        //    euAngle.z = angle;
                                        //    deviantSign.transform.eulerAngles = euAngle;
                                        //}

                                        // Send event message
                                        PushTrialMarker(ring_index, trial, cube_index, freq, c.transform.position, isTarget);

                                        nextSwitchTime = Time.time + duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        //c.GetComponent<SSVEPCue>().Toggle(false);

                                        //if (isTarget)
                                        //{
                                        //    targetSign.SetActive(false);
                                        //}
                                        //else
                                        //{
                                        //    deviantSign.SetActive(false);
                                        //}

                                        arrow.SetActive(false);

                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        if (back_arrow)
                                        {
                                            back_arrow_obj.SetActive(true);
                                            // set back arrow
                                            //GameObject ci = rings[0][cube_index];

                                            // rotation
                                            back_arrow_obj.transform.eulerAngles = new Vector3(0f, 0f, cube_index * 90.0f);

                                            // location
                                            Vector3 arrow_pos = c.transform.position;
                                            arrow_pos.z += (-0.14f);

                                            if (cube_index != 3)
                                            {
                                                arrow_pos.x += (cube_index - 1) * 0.24f;
                                            }
                                            if (cube_index != 0)
                                            {
                                                arrow_pos.y += (cube_index - 2) * 0.24f;
                                            }

                                            back_arrow_obj.transform.position = arrow_pos;

                                        }

                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                            }
                            break;

                        case 3: // Baseline
                            switch (currentBaselineState)
                            {
                                case BaselineState.Stop:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        currentBaselineState = BaselineState.Rest;
                                        switching = true;
                                    }

                                    break;
                                case BaselineState.Rest:
                                    if (switching)
                                    {
                                        if (custom_rest_duration < 0)
                                        {
                                            nextSwitchTime = Time.time + rest_duration;
                                        }
                                        else
                                        {
                                            nextSwitchTime = Time.time + custom_rest_duration;
                                            custom_rest_duration = -1.0f;
                                        }
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        if (trial < 4 * 4)
                                        {
                                            currentBaselineState = BaselineState.ShowSSVEPTarget;
                                        }
                                        else if (trial < 4 * 4 + 6 + 1)
                                        {
                                            currentBaselineState = BaselineState.ShowPLRTarget1;
                                        }
                                        else if (trial < 4 * 4 + 6 + 2)
                                        {
                                            currentBaselineState = BaselineState.ShowPLRTarget2;
                                        }
                                        else if (trial < 4 * 4 + 6 + 2 + 1)
                                        {
                                            currentBaselineState = BaselineState.ShowPLRTarget3;
                                        }
                                        else if (trial < 4 * 4 + 6 + 2 + 2)
                                        {
                                            currentBaselineState = BaselineState.ShowPLRTarget4;
                                        }
                                        else
                                        {
                                            currentBaselineState = BaselineState.Stop;
                                            started = false;
                                        }

                                        switching = true;
                                    }

                                    break;
                                case BaselineState.ShowPLRTarget1:
                                    if (switching)
                                    {
                                        // Show stuff

                                        // Two types of baseline section: 
                                        // trial - 4*4 (removing ssvep) < 6 (first six trial, flash cube and fixation cross) 

                                        if (trial - 4 * 4 < 6)
                                        {
                                            // 100ms 
                                            float PLRduration = 0.1f;
                                            // Show corresponding things
                                            // Cube
                                            baseCubes[0].SetActive(true);

                                            baseCubes[0].GetComponent<SSVEPCueNew>().InvertColor();
                                            baseCubes[0].GetComponent<SSVEPCueNew>().Toggle(false);

                                            // Send event message
                                            PushMarker($"PLR baseline1: Trial {trial - (4 * 4)}", alsoLog: true);

                                            nextSwitchTime = Time.time + PLRduration;
                                            switching = false;
                                        }
                                        else
                                        {
                                            // 100ms 
                                            float PLRduration = 6.0f;
                                            // Show corresponding things
                                            // Cube
                                            baseCubes[0].SetActive(true);

                                            baseCubes[0].GetComponent<SSVEPCueNew>().Toggle(true);
                                            baseCubes[0].GetComponent<PLRCue>().Toggle(true);

                                            // Send event message
                                            PushMarker($"PLR baseline1: Trial {trial - (4 * 4)}", alsoLog: true);

                                            nextSwitchTime = Time.time + PLRduration;
                                            switching = false;
                                        }

                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        baseCubes[0].SetActive(false);

                                        if (trial - 4 * 4 < 6)
                                        {
                                            // 3s 
                                            custom_rest_duration = 3.0f;
                                            // Increment Trial
                                            trial += 1;

                                            // Send event message
                                            PushMarker("End Trial", alsoLog: true);

                                            currentBaselineState = BaselineState.Rest;
                                            switching = true;
                                        }
                                        else
                                        {
                                            // 3s 
                                            custom_rest_duration = 2.0f;
                                            // Increment Trial
                                            trial += 1;

                                            // Send event message
                                            PushMarker("End Trial", alsoLog: true);

                                            currentBaselineState = BaselineState.Rest;
                                            switching = true;
                                        }
                                    }
                                    break;

                                case BaselineState.ShowPLRTarget2:
                                    if (switching)
                                    {
                                        // Show stuff

                                        // Two types of baseline section: 
                                        // trial - 4*4 (removing ssvep) + 6 (first six trial, flash cube and fixation cross) + 1 (6 second)

                                        // 100ms 
                                        float PLRduration = 6.0f;
                                        // Show corresponding things
                                        // Cube
                                        baseCubes[0].SetActive(true);

                                        //baseCubes[0].GetComponent<SSVEPCueNew>().Toggle(false);
                                        baseCubes[0].GetComponent<SSVEPCueNew>().InvertColor();
                                        baseCubes[0].GetComponent<PLRCue>().Toggle(true);

                                        // Send event message
                                        PushMarker("PLR baseline2: Trial " + (trial - (4*4+6+1)), alsoLog: true);

                                        nextSwitchTime = Time.time + PLRduration;
                                        switching = false;

                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        baseCubes[0].SetActive(false);

                                        // 3s 
                                        custom_rest_duration = 2.0f;
                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentBaselineState = BaselineState.Rest;
                                        switching = true;

                                    }
                                    break;

                                case BaselineState.ShowPLRTarget3:
                                    if (switching)
                                    {
                                        // Show stuff

                                        // Two types of baseline section: 
                                        // trial - 4*4 (removing ssvep) + 6 (first six trial, flash cube and fixation cross) + 1 (6 second)

                                        // 100ms 
                                        float PLRduration = 6.0f;
                                        // Show corresponding things
                                        // Cube
                                        baseCubes[0].SetActive(true);

                                        baseCubes[0].GetComponent<SSVEPCueNew>().Toggle(true);
                                        //baseCubes[0].GetComponent<SSVEPCueNew>().InvertColor();
                                        baseCubes[0].GetComponent<PLRCue>().Toggle(true);
                                        baseCubes[0].GetComponent<PLRCue>().ChangeMode((int)PLRCue.PLRMode.Amplitude);

                                        // Send event message
                                        PushMarker("PLR baseline3: Trial " + (trial - (4*4+6+2)), alsoLog: true);

                                        nextSwitchTime = Time.time + PLRduration;
                                        switching = false;

                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        baseCubes[0].SetActive(false);

                                        // 3s 
                                        custom_rest_duration = 2.0f;
                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentBaselineState = BaselineState.Rest;
                                        switching = true;

                                    }
                                    break;

                                case BaselineState.ShowPLRTarget4:
                                    if (switching)
                                    {
                                        // Show stuff

                                        // Two types of baseline section: 
                                        // trial - 4*4 (removing ssvep) + 6 (first six trial, flash cube and fixation cross) + 1 (6 second)

                                        // 100ms 
                                        float PLRduration = 6.0f;
                                        // Show corresponding things
                                        // Cube
                                        baseCubes[0].SetActive(true);

                                        baseCubes[0].GetComponent<SSVEPCueNew>().Toggle(false);
                                        baseCubes[0].GetComponent<SSVEPCueNew>().InvertColor();
                                        baseCubes[0].GetComponent<PLRCue>().Toggle(true);
                                        baseCubes[0].GetComponent<PLRCue>().ChangeMode((int)PLRCue.PLRMode.Amplitude);

                                        // Send event message
                                        PushMarker("PLR baseline4: Trial " + (trial - (4*4+6+3)), alsoLog: true);

                                        nextSwitchTime = Time.time + PLRduration;
                                        switching = false;

                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        baseCubes[0].SetActive(false);

                                        // 3s 
                                        custom_rest_duration = 2.0f;
                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentBaselineState = BaselineState.Rest;
                                        switching = true;

                                    }
                                    break;
                                case BaselineState.ShowSSVEPTarget:

                                    if (switching)
                                    {
                                        // Show stuff

                                        // Show corresponding things
                                        // Cube

                                        baseCubes[(int)trial / 4].SetActive(true);

                                        baseCubes[(int)trial / 4].GetComponent<SSVEPCueNew>().Toggle(true);

                                        int freq = 0;
                                        if (useNewSSVEP)
                                        {
                                            if (baseCubes[(int)trial / 4].GetComponent<SSVEPCueNew>())
                                            {
                                                freq = (int)baseCubes[(int)trial / 4].GetComponent<SSVEPCueNew>().frequency;
                                            }
                                        }
                                        else
                                        {
                                            if (baseCubes[(int)trial / 4].GetComponent<SSVEPCue>())
                                            {
                                                freq = (int)baseCubes[(int)trial / 4].GetComponent<SSVEPCue>().frequency;
                                            }
                                        }

                                        // Send event message
                                        PushMarker("SSVEP baseline: Trial " + (trial) + "; Frequency: " + (freq), alsoLog: true);

                                        nextSwitchTime = Time.time + duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        baseCubes[(int)trial / 4].SetActive(false);

                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentBaselineState = BaselineState.Rest;
                                        switching = true;
                                    }

                                    break;
                            }
                            break;

                        case 4: // SSVEP 1: arrow
                            switch (currentState)
                            {
                                case State.Stop:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                                case State.Center:
                                    if (switching)
                                    {
                                        nextSwitchTime = Time.time + rest_duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        if (trial >= trial_count)
                                        {
                                            // Increment ring
                                            trial = 0;
                                            //ring_index += 1;
                                        }

                                        if (ring_index >= rings.Count)
                                        {
                                            // End
                                            currentState = State.Stop;
                                            started = false;
                                        }
                                        else
                                        {
                                            currentState = State.ShowTarget;
                                        }

                                        switching = true;
                                    }

                                    break;
                                case State.ShowTarget:

                                    if (switching)
                                    {
                                        // Show stuff

                                        // Check standard or deviant
                                        float o = Random.Range(0.0f, 1.0f);
                                        if (o >= odd)
                                        {
                                            isTarget = false;
                                        }
                                        else
                                        {
                                            isTarget = true;
                                        }

                                        // Check which cube to show
                                        cube_index = Random.Range(0, rings[ring_index].Count);

                                        // Show corresponding things
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        c.SetActive(true);
                                        if (useNewSSVEP)
                                        {
                                            c.GetComponent<SSVEPCueNew>().Toggle(true);
                                        }
                                        else
                                        {
                                            c.GetComponent<SSVEPCue>().Toggle(true);
                                        }

                                        // set center arrow
                                        arrow.SetActive(true);
                                        GameObject ci = rings[0][cube_index];

                                        // rotation
                                        arrow.transform.eulerAngles = new Vector3(0f, 0f, cube_index * 90.0f + 180.0f);

                                        // location
                                        Vector3 arrow_pos = ci.transform.position;
                                        arrow_pos.z += (-0.07f);
                                        arrow.transform.position = arrow_pos;

                                        // Sign
                                        if (isTarget)
                                        {
                                            targetSign.SetActive(true);
                                            // Move Sign
                                            Vector3 pos = c.transform.position;
                                            pos.z += (-0.06f);
                                            targetSign.transform.position = pos;
                                        }
                                        else
                                        {
                                            deviantSign.SetActive(true);
                                            // Move Sign
                                            Vector3 pos = c.transform.position;
                                            pos.z += (-0.06f);
                                            deviantSign.transform.position = pos;
                                        }

                                        // Send event message
                                        PushTrialMarker(ring_index, trial, cube_index, c.transform.position, isTarget);

                                        nextSwitchTime = Time.time + duration;
                                        switching = false;
                                    }

                                    if (Time.time > nextSwitchTime)
                                    {
                                        // Hide everything
                                        // Cube
                                        GameObject c = rings[ring_index][cube_index];
                                        if (useNewSSVEP)
                                        {
                                            c.GetComponent<SSVEPCueNew>().Toggle(false);
                                        }
                                        else
                                        {
                                            c.GetComponent<SSVEPCue>().Toggle(false);
                                        }

                                        if (isTarget)
                                        {
                                            targetSign.SetActive(false);
                                        }
                                        else
                                        {
                                            deviantSign.SetActive(false);
                                        }

                                        arrow.SetActive(false);

                                        // Increment Trial
                                        trial += 1;

                                        // Send event message
                                        PushMarker("End Trial", alsoLog: true);

                                        currentState = State.Center;
                                        switching = true;
                                    }

                                    break;
                            }
                            break;
                    }

                    break;
                case Mode.two_step:
                    switch (currentState)
                    {
                        case State.Stop:
                            if (switching)
                            {
                                nextSwitchTime = Time.time + rest_duration;
                                switching = false;
                            }

                            if (Time.time > nextSwitchTime)
                            {
                                currentState = State.Center;
                                switching = true;
                            }

                            break;
                        case State.Center:
                            if (switching)
                            {
                                nextSwitchTime = Time.time + rest_duration;
                                switching = false;
                            }

                            if (Time.time > nextSwitchTime)
                            {
                                if (trial >= trial_count)
                                {
                                    // Increment ring
                                    trial = 0;
                                    //ring_index += 1;
                                }

                                if (ring_index >= rings.Count)
                                {
                                    // End
                                    currentState = State.Stop;
                                    started = false;
                                }
                                else
                                {
                                    currentState = State.ShowTarget;
                                }

                                switching = true;
                            }

                            break;
                        case State.ShowTarget:

                            if (switching)
                            {
                                // Show stuff

                                // Check standard or deviant
                                float o = Random.Range(0.0f, 1.0f);
                                if (o >= odd)
                                {
                                    isTarget = false;
                                }
                                else
                                {
                                    isTarget = true;
                                }

                                // Check which cube to show
                                cube_index = Random.Range(0, rings[ring_index].Count);

                                // Show corresponding things
                                // Cube
                                GameObject c = rings[ring_index][cube_index];
                                c.SetActive(true);
                                int freq = 0;
                                if (useNewSSVEP)
                                {
                                    c.GetComponent<SSVEPCueNew>().Toggle(true);
                                    freq = (int)c.GetComponent<SSVEPCueNew>().frequency;
                                }
                                else
                                {
                                    if (c.GetComponent<SSVEPCue>())
                                    {
                                        c.GetComponent<SSVEPCue>().Toggle(ssvep);
                                        freq = (int)c.GetComponent<SSVEPCue>().frequency;
                                    }
                                }

                                // Sign
                                if (isTarget)
                                {
                                    //targetSign.SetActive(true);
                                    // Move Sign
                                    Vector3 pos = c.transform.position;
                                    pos.z += (-0.06f);
                                    targetSign.transform.position = pos;
                                }
                                else
                                {
                                    //deviantSign.SetActive(true);
                                    // Move Sign
                                    Vector3 pos = c.transform.position;
                                    pos.z += (-0.06f);
                                    deviantSign.transform.position = pos;
                                }

                                // Send event message
                                PushTrialMarker(ring_index, trial, cube_index, freq, c.transform.position, isTarget);

                                nextSwitchTime = Time.time + duration;
                                switching = false;
                            }

                            if (Time.time > nextSwitchTime)
                            {
                                // Hide everything
                                // Cube
                                GameObject c = rings[ring_index][cube_index];

                                // save last position
                                lastPosition = c.transform.position;
                                c.SetActive(false);

                                if (isTarget)
                                {
                                    targetSign.SetActive(false);
                                }
                                else
                                {
                                    deviantSign.SetActive(false);
                                }

                                // Increment Trial
                                //trial += 1;

                                // Send event message
                                PushMarker("First Cube End", alsoLog: true);

                                currentState = State.SecondCube;
                                switching = true;
                            }

                            break;
                        case State.SecondCube:
                            if (switching)
                            {
                                // randomize direction
                                float o = Random.Range(0.0f, 4.0f);
                                Vector3 targetPos = lastPosition;
                                if (o < 1.0f)
                                {
                                    // left
                                    targetPos.x -= distance;

                                }
                                else if (o >= 1.0f && o < 2.0f)
                                {
                                    // top
                                    targetPos.y += distance;
                                }
                                else if (o >= 2.0f && o < 3.0f)
                                {
                                    // right
                                    targetPos.x += distance;
                                }
                                else if (o >= 3.0f && o < 4.0f)
                                {
                                    // bottom
                                    targetPos.y -= distance;
                                }

                                secondCube.transform.parent = null;
                                secondCube.transform.rotation = Quaternion.identity;
                                secondCube.transform.position = targetPos;
                                secondCube.SetActive(true);
                                secondCube.transform.parent = Camera.main.gameObject.transform;

                                PushSecondCubeMarker(ring_index, trial, targetPos);

                                nextSwitchTime = Time.time + duration;
                                switching = false;
                            }

                            if (Time.time > nextSwitchTime)
                            {
                                // Hide everything
                                // Cube
                                secondCube.SetActive(false);

                                // Increment Trial
                                trial += 1;

                                // Send event message
                                PushMarker("End Trial", alsoLog: true);

                                currentState = State.Center;
                                switching = true;
                            }

                            break;
                    }

                    break;
            }

        }
        else
        {
            if (!resetted)
            {
                Reset();
            }
        }
    }

    public void Reset()
    {
        //SetMode();
        currentState = State.Stop;
        currentBaselineState = BaselineState.Stop;
        controllerPressed = false;
        targetSign.SetActive(false);
        deviantSign.SetActive(false);
        leftQuadSign.SetActive(false);
        rightQuadSign.SetActive(false);
        leftQuadSigns.ForEach(x => x.SetActive(false));
        rightQuadSigns.ForEach(x => x.SetActive(false));
        arrow.SetActive(false);
        if (back_arrow_obj)
        {
            back_arrow_obj.SetActive(false);
        }
        if (useNewSSVEP)
        {
            rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCueNew>().Toggle(false)));
        }
        else
        {
            rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCue>().Toggle(false)));
        }

        switch (dropdown_mode.value)
        {
            case 0:
                r1Cubes.ForEach(x => x.SetActive(false));
                r2Cubes.ForEach(x => x.SetActive(false));
                r3Cubes.ForEach(x => x.SetActive(false));
                rings.ForEach(x => x.ForEach(y => y.SetActive(false)));
                break;
            case 1:
                rings.ForEach(x => x.ForEach(y => y.SetActive(false)));

                rings[ring_index].ForEach(y => y.SetActive(true));
                if (useNewSSVEP)
                {
                    rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCueNew>().Toggle(true)));
                }
                else
                {
                    rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCue>().Toggle(true)));
                }

                //rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCue>().Toggle(false)));
                break;
            case 2:
                rings.ForEach(x => x.ForEach(y => y.SetActive(false)));

                rings[ring_index].ForEach(y => y.SetActive(true));
                if (useNewSSVEP)
                {
                    rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCueNew>().Toggle(true)));
                }
                else
                {
                    rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCue>().Toggle(true)));
                }

                //rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCue>().Toggle(false)));
                break;
            //case 2:
            //    rings.ForEach(x => x.ForEach(y => y.SetActive(false)));
            //    //rings.ForEach(x => x.ForEach(y => y.GetComponent<SSVEPCue>().Toggle(false)));
            //    break;
            case 3:
                rings.ForEach(x => x.ForEach(y => y.SetActive(false)));
                break;
            case 4:
                rings[2].ForEach(y => y.SetActive(true));
                break;
        }

        if (dropdown_mode.value != 0)
        {
            AlignShapeToCurrentRing();
        }

        resetted = true;
    }

    void AlignShapeToCurrentRing()
    {
        for (int i = 0; i < 4; i++)
        {
            var left = leftQuadSigns[i];
            var right = rightQuadSigns[i];
            var tar = rings[ring_index][i];
            // Move Sign
            Vector3 pos = tar.transform.position;
            pos.z += (-0.06f);
            left.transform.position = pos;
            right.transform.position = pos;
        }
    }

    void RandomRotationAndColor(bool rot, bool color, List<Color> possibleColors)
    {
        leftQuadSigns.ForEach(x => x.SetActive(false));
        rightQuadSigns.ForEach(x => x.SetActive(false));
        for (int i = 0; i < 4; i++)
        {
            bool isLeft = false;
            // Check standard or deviant
            float o = Random.Range(0.0f, 1.0f);
            if (o >= odd)
            {
                isLeft = false;
            }
            else
            {
                isLeft = true;
            }

            if (isLeft)
            {
                leftQuadSigns[i].SetActive(true);
            }
            else
            {
                rightQuadSigns[i].SetActive(true);
            }

            if (rot)
            {

                float angle = Random.Range(0, 360.0f);
                // Sign
                if (isLeft)
                {
                    // Move Sign
                    Vector3 euAngle = leftQuadSigns[i].transform.eulerAngles;
                    euAngle.z = angle;
                    leftQuadSigns[i].transform.eulerAngles = euAngle;
                }
                else
                {
                    // Move Sign
                    Vector3 euAngle = rightQuadSigns[i].transform.eulerAngles;
                    euAngle.z = angle;
                    rightQuadSigns[i].transform.eulerAngles = euAngle;
                }
            }

        }

        if (color)
        {
            int index = Random.Range(0, possibleColors.Count);
            var newColor = possibleColors[index];

            leftQuadSigns.ForEach(x => x.GetComponent<Renderer>().material.color = newColor);
            rightQuadSigns.ForEach(x => x.GetComponent<Renderer>().material.color = newColor);
        }
    }

    public void Set2D(bool flag)
    {
        is_2d = flag;
        if (flag)
        {
            PushMarker("2D enabled");
            // move dummy_camera to certain distance from the target_center
            dummy_camera.position = target_center.position + new Vector3(0, 0, -dist_2d);
            display.t = dummy_camera;
        }
        else
        {
            PushMarker("2D disabled");
            display.t = vr_camera;
        }
    }

    public void SetDistance(float dist)
    {
        dist_2d = dist;
        dummy_camera.transform.position = target_center.transform.position + new Vector3(0, 0, -dist_2d);

        // update the text with format 2D: {0:0.00}m
        text_2d.text = string.Format("2D: {0:0.00}m", dist_2d);
    }

    public void SendEvent(string[] events)
    {
        markerStream.push_sample(events);
    }
}
