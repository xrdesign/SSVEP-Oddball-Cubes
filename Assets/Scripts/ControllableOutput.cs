using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using VRTK.Controllables;

public class ControllableOutput : MonoBehaviour
{
    public VRTK_BaseControllable controllable;
    public bool isButton;
    public Text displayText;
    public string outputDisplay = "Controllable Triggered";
    public string outputOnMax = "Maximum Reached";
    public string outputOnMin = "Minimum Reached";
    public UnityEvent triggerMethods;

    bool methodsNotNull = true;
    int startResetTime = 0;

    protected virtual void OnEnable()
    {
        controllable = (controllable == null ? GetComponent<VRTK_BaseControllable>() : controllable);
        controllable.ValueChanged += ValueChanged;
        controllable.MaxLimitReached += MaxLimitReached;
        controllable.MinLimitReached += MinLimitReached;

        for (int i = 0; i < triggerMethods.GetPersistentEventCount(); i++)
        {
            if (triggerMethods.GetPersistentTarget(i) == null)
            {
                Debug.LogError("Method Has Null in Controllable Output");
                methodsNotNull = false;
                break;
            }
        }
        
    }

    protected virtual void ValueChanged(object sender, ControllableEventArgs e)
    {
        if (displayText != null && !isButton)
        {
            displayText.text = e.value.ToString("0");
        }
    }

    protected virtual void MaxLimitReached(object sender, ControllableEventArgs e)
    {
        if (isButton)
        {
            if (startResetTime > 0)
            {
                startResetTime--;
            }
            else if (methodsNotNull)
            {
                triggerMethods.Invoke();
                startResetTime = 5;
            }
            if (displayText != null)
            {
                displayText.text = (outputDisplay != "" ? outputDisplay : "buttonPress");
            }
        }
        

        if (outputOnMax != "")
        {
            Debug.Log(outputOnMax);
        }
    }

    protected virtual void MinLimitReached(object sender, ControllableEventArgs e)
    {
        if (outputOnMin != "")
        {
            Debug.Log(outputOnMin);
        }
    }
}