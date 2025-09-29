using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Assertions.Comparers;
using UnityEngine.SpatialTracking;
using UnityEngine.UI;
using Valve.VR;
using VRTK;

public class ReplayManager : MonoBehaviour
{
    #region Serialization

    [System.Serializable]
    public class Frame : System.Object
    {
        // 4 + 4 + 4 * 3 + 4 * 4 = 36
        [SerializeField] public string uniqueID;
        [SerializeField] public float timestamp;
        [SerializeField] public Vector3 position;
        [SerializeField] public Quaternion rotation;
    }

    private BinaryFormatter bf;
    private SurrogateSelector surrogateSelector;
    private Vector3Surrogate vector3Surrogate;
    private QuaternionSurrogate quaternionSurrogate;

    #endregion

    #region Enum

    public enum Mode
    {
        Replay,
        Record,
        ModeCount
    }

    public enum State
    {
        Play,
        Pause,
        Stop,
        StateCount
    }

    #endregion

    //[HideInInspector]

    #region Filename Variable

    private string replayFileName = "";
    private string defaultFileName = "Replay.dem";
    private string currentFileName = "";

    #endregion

    #region State Variable

    private Mode currentMode = Mode.Replay;
    private State currentState = State.Stop;

    private bool initialized = false;
    private bool isStopped = false;
    private bool isPlayed = false;
    private bool isReplayInitialized = false;

    #endregion

    #region Debug Variable

    private string lastError = "Success";
    private bool isDebug = true;
    public bool forcedUpdate = false;

    #endregion

    #region Progess Variable

    private float currentTime;
    private float deltaTime = -1;
    private float startTime = -1;
    private float globalTime;
    private string uniqueID;
    private float minTime;
    private float maxTime;
    private float lastTime;

    private bool needRelocate = false;

    public Text minTimeText;
    public Text maxTimeText;
    public Text globalTimeText;
    public Text replayTimeText;
    public Slider progressSlider;

    #endregion

    #region Replay Frame Variable

    private List<UniqueID> gameObjectList;
    [SerializeField] private List<List<Frame>> frameList;
    private List<bool> isChanged;
    private List<int> indices;

    private int currIndex = -1;

    private float speed = 1.0f;
    private float minSpeed = 0.01f;
    private float maxSpeed = 10.0f;

    #endregion

    #region Public Interface

    public string GetLastError(bool reset = true)
    {
        string temp = lastError;
        if (reset)
        {
            lastError = "Success";
        }

        return temp;
    }

    #endregion

    #region Private Helper Function

    void DebugPrint(string s)
    {
        if (isDebug)
        {
            Debug.Log(s);
        }
    }

    void LoadReplay(string filename)
    {
        FileStream file;
        try
        {
            file = File.Open(@"C:\EscapeRoomData" + "/Replay/" + filename, FileMode.Open);

            Debug.Log("Load from: " + @"C:\EscapeRoomData" + "/Replay/" + filename);
            frameList = (List<List<Frame>>) bf.Deserialize(file);
            minTime = frameList[0][0].timestamp;
            maxTime = frameList[0][frameList[0].Count - 1].timestamp;
            foreach (var frames in frameList)
            {
                minTime = Mathf.Min(minTime, frames[0].timestamp);
                maxTime = Mathf.Max(maxTime, frames[frames.Count - 1].timestamp);
            }

            currentFileName = filename;
            file.Close();
            
            initialized = false;
            isReplayInitialized = false;
            gameObjectList = new List<UniqueID>();
        }
        catch (Exception e)
        {
            lastError = "LoadReplay: Exception - fail to load file " + filename;
            Debug.Log(e.ToString());
        }
    }

    void ResetIndex()
    {
        if (indices != null)
        {
            for (int i = 0; i < indices.Count; i++)
            {
                indices[i] = 0;
            }
        }

        needRelocate = false;
    }

    float SliderToTime(float progress)
    {
        if (progress > 100 || progress < 0)
        {
            lastError = "SliderToTime: Invalid progress";
            DebugPrint(lastError);
            return 0;
        }
        else
        {
            float factor = progress / 100.0f;
            return minTime + factor * (maxTime - minTime);
        }
    }

    float TimeToSlider(float time)
    {
        float factor = (time - minTime) / (maxTime - minTime);
        return factor * 100.0f;
    }

    #endregion

    #region UI Function

    public void SetState(Int32 state)
    {
        DebugPrint("SetState to " + state);
        if (state >= 0 && state < (int) State.StateCount)
        {
            currentState = (State) state;
        }
        else
        {
            lastError = "SetState: Invalid state";
        }

        if (currentState != State.Play)
        {
            isPlayed = false;
        }

        if (currentState != State.Stop)
        {
            isStopped = false;
        }
    }

    public void SetMode(Int32 mode)
    {
        DebugPrint("SetMode to " + mode);
        if (mode >= 0 && mode < (int) Mode.ModeCount)
        {
            currentMode = (Mode) mode;
            initialized = false;
            isReplayInitialized = false;
            gameObjectList = new List<UniqueID>();
            frameList = new List<List<Frame>>();
        }
        else
        {
            lastError = "SetMode: Invalid mode";
        }
    }

    public void SetFileName(string s)
    {
        DebugPrint("SetFileName to " + s);
        replayFileName = s;
    }

    public void SaveCurrentDemo()
    {
        Debug.Log("The size of replay is: " + frameList.Count * frameList[0].Count);

        var list = new List<List<Frame>>();
        for (int i = 0; i < frameList.Count; i++)
        {
            if (isChanged[i])
            {
                list.Add(frameList[i]);
            }
        }

        FileStream file = File.Create(@"C:\EscapeRoomData" + "/Replay/" + replayFileName);
        Debug.Log("Save to: " + @"C:\EscapeRoomData" + "/Replay/" + replayFileName);
        if (bf != null) bf.Serialize(file, list);
        file.Close();
    }

    public void LoadDemo()
    {
        DebugPrint("Loading demo:" + replayFileName);
        if (replayFileName == "")
        {
            LoadReplay("Replay.dem");
        }
        else
        {
            LoadReplay(replayFileName);
        }
    }

    private void UpdateUITimer()
    {
        try
        {
            minTimeText.text = minTime.ToString();
            maxTimeText.text = maxTime.ToString();
            globalTimeText.text = globalTime.ToString();
            replayTimeText.text = currentTime.ToString();
        }
        catch (Exception)
        {
            Debug.Log("UI elements are not assigned properly");
        }
    }

    #endregion

    #region Unity Function

    // Use this for initialization
    void Start()
    {
        if (!GetComponent<UniqueID>())
        {
            UniqueID u = this.gameObject.AddComponent<UniqueID>();
            if (string.IsNullOrEmpty(u.guid))
            {
                Guid guid = Guid.NewGuid();
                u.guid = guid.ToString();
            }

            Debug.Log("Add UniqueID " + u.guid + " to: " + this.name);
        }

        Debug.Log("My instance ID:" + this.gameObject.GetComponent<UniqueID>().guid);
        uniqueID = this.gameObject.GetComponent<UniqueID>().guid;
        gameObjectList = new List<UniqueID>();
        frameList = new List<List<Frame>>();

        // serializer 
        bf = new BinaryFormatter();
        surrogateSelector = new SurrogateSelector();
        vector3Surrogate = new Vector3Surrogate();
        quaternionSurrogate = new QuaternionSurrogate();
        surrogateSelector.AddSurrogate(typeof(Vector3), new StreamingContext(StreamingContextStates.All),
            vector3Surrogate);
        surrogateSelector.AddSurrogate(typeof(Quaternion), new StreamingContext(StreamingContextStates.All),
            quaternionSurrogate);
        bf.SurrogateSelector = surrogateSelector;

        SetState((int) State.Stop);
    }

    void FixedUpdate()
    {
        globalTime = Time.fixedUnscaledTime;
    }

    void LateUpdate()
    {

        switch (currentState)
        {
            case State.Play:
                currentTime += Time.unscaledDeltaTime;
                break;
            case State.Pause:
                break;
            case State.Stop:
                break;
        }

        switch (currentMode)
        {
             case Mode.Record:
                 Record();
                 break;
             case Mode.Replay:

                 if (currentTime > maxTime)
                 {
                     if (currentState == State.Play)
                     {
                         SetState((int) State.Pause);
                     }
                 }
                 
                 Replay();
                 break;
        }

        UpdateUITimer();
    }

    #endregion

    void UpdateScene(bool isReplay)
    {
        UniqueID[] gameObjectList = Resources.FindObjectsOfTypeAll<UniqueID>();
        foreach (var gameObject in gameObjectList)
        {
            if (gameObject.guid == "")
            {
                Debug.LogWarning("This object has problematic guid: " + gameObject.name + ": " + gameObject.guid);
            }

            if (gameObject.guid != uniqueID && gameObject.tag != "ignore" && gameObject.guid != "")
            {
                if (!this.gameObjectList.Contains(gameObject))
                {
                    Debug.Log("Adding " + gameObject.name + ": " + gameObject.guid);
                    this.gameObjectList.Add(gameObject);
                    gameObject.transform.hasChanged = false;
                    if (isReplay)
                    {
                    }
                    else
                    {
                        this.frameList.Add(new List<Frame>());
                    }
                }
            }
        }
    }

    void Record()
    {
        if (currentState == State.Pause || currentState == State.Stop)
        {
            return;
        }

        if (!initialized || forcedUpdate)
        {
            UpdateScene(false);
            if (!initialized)
            {
                isChanged = Enumerable.Repeat(false, frameList.Count).ToList();
                minTime = currentTime = 0;
                initialized = true;
            }
        }

        if (Mathf.Abs(currentTime - lastTime) < 0.033)
        {
            return;
        }

        lastTime = currentTime;

        for (int i = 0; i < gameObjectList.Count; i++)
        {
            GameObject gameObject = gameObjectList[i].gameObject;
            if (gameObject.transform.hasChanged)
            {
                if (!isChanged[i] && frameList[i].Count > 1)
                {
                    var diffRot = Quaternion.Angle(gameObject.transform.rotation, frameList[i][0].rotation);
                    var diff = gameObject.transform.position - frameList[i][0].position;
                    if (diff.magnitude >= 0.05 || diffRot >= 10.0)
                    {
                        isChanged[i] = true;
                    }
                }

                Frame frame = new Frame
                {
                    timestamp = currentTime - minTime,
                    uniqueID = gameObject.GetComponent<UniqueID>().guid,
                    position = gameObject.transform.position,
                    rotation = gameObject.transform.rotation
                };
                frameList[i].Add(frame);

                gameObject.transform.hasChanged = false;
            }
        }

        maxTime = currentTime;
    }

    void Replay()
    {
        if (!initialized)
        {
            indices = Enumerable.Repeat(0, frameList.Count).ToList();
            UpdateScene(true);
            initialized = true;
        }

        if (!isReplayInitialized)
        {
            currentTime = minTime;
            ResetIndex();

            isReplayInitialized = true;
        }

        switch (currentState)
        {
            case State.Play:
                if (!isPlayed)
                {
                    currentTime = minTime + deltaTime;
                    isPlayed = true;
                }
                deltaTime = currentTime - minTime;
                progressSlider.value = TimeToSlider(deltaTime);
                break;
            case State.Pause:
                if (!FloatComparer.AreEqual(progressSlider.value, deltaTime, 0.01f))
                {
                    ResetIndex();
                }
                deltaTime = SliderToTime(progressSlider.value);
                break;
            case State.Stop:
                if (!isStopped)
                {
                    progressSlider.value = 0;
                    isStopped = true;
                    isReplayInitialized = false;
                }
                if (!FloatComparer.AreEqual(progressSlider.value, deltaTime, 0.01f))
                {
                    ResetIndex();
                }
                deltaTime = SliderToTime(progressSlider.value);
                break;
        }

        if (currentState == State.Stop)
        {
            return;
        }

        for (int i = 0; i < frameList.Count; i++)
        {
            string guid = frameList[i][0].uniqueID;
            UniqueID currId = gameObjectList.Find(x => x.guid == guid);
            if (currId == null)
            {
                // the gameObject corresponding to current frameList is no longer exist
                continue;
            }

            GameObject curr = currId.gameObject;

            Rigidbody r = curr.GetComponent<Rigidbody>();
            if (r)
            {
                r.isKinematic = true;
            }

            // Start from index
            List<Frame> frames = this.frameList[i];
            for (int j = indices[i]; j < frames.Count; j++)
            {
                Frame currFrame = frames[j];
                if (currFrame.timestamp < deltaTime)
                {
                    continue;
                }

                // Correct Frame (j and j - 1)
                if (j > 0)
                {
                    // Interpolate j and j - 1 frame
                    Frame lastFrame = frames[j - 1];

                    // Calculate (s)lerp factor
                    float factor = (deltaTime - lastFrame.timestamp) / (currFrame.timestamp - lastFrame.timestamp);

                    // (s)lerp
                    curr.transform.position = Vector3.LerpUnclamped(lastFrame.position, currFrame.position, factor);
                    curr.transform.rotation =
                        Quaternion.LerpUnclamped(lastFrame.rotation, currFrame.rotation, factor);
                }
                else
                {
                    // first key (no interpolation needed)
                    curr.transform.position = currFrame.position;
                    curr.transform.rotation = currFrame.rotation;
                }

                indices[i] = j;
                break;
            }
        }
    }
}