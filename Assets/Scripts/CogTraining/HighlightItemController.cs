using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HighlightItemController : MonoBehaviour
{

    public LSLStreamer lslStreamer;

    public enum HighlightMode
    {
        None,
        Tags,
        Volume,
        Name,
        Count
    }

    public List<ItemTags> itemList = new List<ItemTags>();

    public HighlightMode currentHighlightMode = HighlightMode.Tags;

    public MaskedEnum highlighted = 0;

    [Range(0.01f, 1f)]
    public float volumeThresholdMin = 0.4f;
    
    [Range(0.01f, 1f)] 
    public float volumeThresholdMax = 0.5f;

    public string highlightName = "product";

    public HintController hintController; // to hide actual objects when the showinghint == true

    public float delayAfterHint = 5.0f; // how long do we wait before showing the items after the hint is disabled
    private float timeSinceHintDisabled = 0.0f;

    private bool refreshing = false;
    
    public float timer = 0.0f;
    public Text timeText;

    // Start is called before the first frame update
    void Start()
    {
        UpdateList(0);
    }

    public void UpdateList(float delay)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            if (c.GetComponent<ItemTags>())
            {
                itemList.Add(c.GetComponent<ItemTags>());
            }
        }

        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].gameObject.SetActive(true);
            if (itemList[i].gameObject.GetComponent<Renderer>())
            {
                itemList[i].gameObject.GetComponent<Renderer>().enabled = false;
            }

            // disable renderer in children as well
            var list = itemList[i].GetComponentsInChildren<Renderer>();
            foreach (var r in list)
            {
                r.enabled = false;
            }
        }

        refreshing = true;
        StartCoroutine(EnableItems(delay));
    }
    
    // Coroutine to enable itemList item after a delay
    IEnumerator EnableItems(float delay)
    {
        yield return new WaitForSeconds(delay);
        for (int i = 0; i < itemList.Count; i++)
        {
            if (itemList[i].gameObject.GetComponent<Renderer>())
            {
                itemList[i].gameObject.GetComponent<Renderer>().enabled = true;
            }

            // enable renderer in children as well
            var list = itemList[i].GetComponentsInChildren<Renderer>();
            foreach (var r in list)
            {
                r.enabled = true;
            }
        }
        refreshing = false;

    }

    public void CleanItems()
    {
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].gameObject.transform.parent = null;
            DestroyImmediate(itemList[i].gameObject);
        }
        itemList.Clear();

        // reset timer
        timer = 0.0f;
    }

    // Update is called once per frame
    void Update()
    {

        // update Text content with value from timer (2 digits after decimal point)
        timeText.text = timer.ToString("0.00");

        if (refreshing)
        {
            return;
        }

        if (hintController.showingHint || !hintController.itemSpawner.hintShown)
        {
            // disable all items
            for (int i = 0; i < itemList.Count; i++)
            {
                // set renderer to false if it has one
                if (itemList[i].gameObject.GetComponent<Renderer>())
                {
                    itemList[i].gameObject.GetComponent<Renderer>().enabled = false;
                }

                // enable renderer in children as well
                var list = itemList[i].GetComponentsInChildren<Renderer>();
                foreach (var r in list)
                {
                    r.enabled = false;
                }                
            }

            // reset time since hint disabled
            timeSinceHintDisabled = 0.0f;
        }
        else
        {
            timeSinceHintDisabled += Time.deltaTime;
            if (timeSinceHintDisabled > delayAfterHint)
            {
                // enable all items
                for (int i = 0; i < itemList.Count; i++)
                {
                    if (itemList[i].gameObject.GetComponent<Renderer>())
                    {
                        itemList[i].gameObject.GetComponent<Renderer>().enabled = true;
                    }

                    // enable renderer in children as well
                    var list = itemList[i].GetComponentsInChildren<Renderer>();
                    foreach (var r in list)
                    {
                        r.enabled = true;
                    }
                }

                // increment timer (if there are items)
                if (itemList.Count > 0)
                {
                    timer += Time.deltaTime;
                }
            }
        }


        lslStreamer.SendTimer(timer);

        if (volumeThresholdMax <= volumeThresholdMin)
        {
            volumeThresholdMax = volumeThresholdMin + 0.01f;
        }

        if (volumeThresholdMin >= volumeThresholdMax)
        {
            volumeThresholdMin = volumeThresholdMax - 0.01f;
        }

        switch (currentHighlightMode)
        {
            case HighlightMode.None:
                // just disable all outline
                
                foreach (var c in itemList)
                {
                    var outline = c.GetComponent<Outline>();
                    if (outline)
                    {
                        outline.enabled = false;
                    }

                }

                break;
            case HighlightMode.Tags:
                // use tags to highlight: highlighted

                foreach (var c in itemList)
                {
                    var outline = c.GetComponent<Outline>();
                    if (outline)
                    {
                        if ((c.itemTags & highlighted) != 0)
                        {
                            outline.enabled = true;
                        }
                        else
                        {
                            outline.enabled = false;
                        }
                    }

                }

                break;
            case HighlightMode.Volume:
                // iterate through all items and highlight the ones that has volume larger than volumeThreshold
                foreach (var c in itemList)
                {
                    var outline = c.GetComponent<Outline>();
                    if (outline)
                    {
                        // get volume from renderer bound
                        var renderer = c.GetComponent<Renderer>();
                        if (renderer)
                        {
                            var volume = renderer.bounds.size.x * renderer.bounds.size.y * renderer.bounds.size.z;
                            if (volume > volumeThresholdMin && volume < volumeThresholdMax)
                            {
                                outline.enabled = true;
                            }
                            else
                            {
                                outline.enabled = false;
                            }
                        }
                    }

                }

                break;
            case HighlightMode.Name:
                // use name to highlight: anything that contains the substring in name will be highlighted
                foreach (var c in itemList)
                {
                    var outline = c.GetComponent<Outline>();
                    if (outline)
                    {
                        if (c.name.ToLower().Contains(highlightName))
                        {
                            outline.enabled = true;
                        }
                        else
                        {
                            outline.enabled = false;
                        }
                    }

                }

                break;
        }
    }
}
