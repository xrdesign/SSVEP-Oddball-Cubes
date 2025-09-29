using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemSpawner : MonoBehaviour
{

    public List<GameObject> itemTemplates = new List<GameObject>();

    public HighlightItemController highlightItemController;
    

    public enum Mode
    {
        SimilarVolume,
        SimilarName,
        SimilarTags,
    }

    public Mode mode = Mode.SimilarVolume;
    public int nameLength = 5; // for SimilarName mode
    public int tagsCount = 2; // for SimilarTags mode
    public float deltaVolume = 0.05f; // for SimilarVolume mode

    public int numberOfDistractors = 10;

    public BoxCollider spawnArea;

    public bool isSpawning = false; //TODO

    private int target_ind = -1;

    public float delayInSeconds = 5.0f;

    public GameObject target;

    public bool hintShown = false;

    public LSLStreamer lslStreamer;

    // Start is called before the first frame update
    void Start()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            var c = transform.GetChild(i);
            itemTemplates.Add(c.gameObject);
        }
    }

    public void NextTrial()
    {
        isSpawning = true;
        hintShown = false;

        lslStreamer.SendEvent("Trial starts.");
    }

    private Bounds CalculateLocalBounds(GameObject parent)
    {
        Quaternion currentRotation = parent.transform.rotation;
        parent.transform.rotation = Quaternion.Euler(0f, 0f, 0f);
        Bounds bounds = new Bounds(parent.transform.position, Vector3.zero);
        
        if (parent.GetComponent<Renderer>() != null)
        {
            bounds.Encapsulate(parent.GetComponent<Renderer>().bounds);
        }

        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            bounds.Encapsulate(renderer.bounds);
        }

        Vector3 localCenter = bounds.center - parent.transform.position;
        bounds.center = localCenter;
        Debug.Log("The local bounds of this model is " + bounds);
        parent.transform.rotation = currentRotation;

        return bounds;
    }
    public void RandomSpawnUntilNoCollision(GameObject item, int maxTry = 10)
    {
        int tryCount = 0;
        while (tryCount < maxTry)
        {
            // randomly pick a position
            var position = new Vector3(
                Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
                Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
            );
            
            // get the bounds of the item
            var bounds = CalculateLocalBounds(item);

            // get maxExtents
            var maxExtents = Mathf.Max(bounds.extents.x, bounds.extents.y, bounds.extents.z);

            if (item.GetComponent<Collider>())
            {
                var collider = item.GetComponent<Collider>();
                
                // check if item collides with any collider (ignoring trigger)
                if (!Physics.CheckSphere(position, maxExtents, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore))
                {
                    // if not, return

                    // assign the position to the item gameobject transform
                    item.transform.position = position;

                    return;
                }
            }

            // increment try count
            tryCount++;
        }
    }
    
    
    // Update is called once per frame
    void Update()
    {
        if (isSpawning)
        {
            // clean the current highlight item controller
            highlightItemController.CleanItems();

            // randomly pick one item from the templates as target
            target_ind = Random.Range(0, itemTemplates.Count);
            target = itemTemplates[target_ind];

            // randomly pick a point within the spawn area
            var point = new Vector3(
                Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
                Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
            );

            // randomly pick an rotation
            var rotation = Quaternion.Euler(
                Random.Range(0, 360),
                Random.Range(0, 360),
                Random.Range(0, 360)
            );

            // instantiate the target at the point
            target = Instantiate(target, point, rotation);
            target.transform.parent = highlightItemController.transform;

            RandomSpawnUntilNoCollision(target);

            // randomly spawn multiple other items as distractor
            for (int i = 0; i < numberOfDistractors; i++)
            {
                int rand_ind = Random.Range(0, itemTemplates.Count);
                if (rand_ind == target_ind)
                {
                    rand_ind = (rand_ind + 1) % itemTemplates.Count;
                }
                var distractor = itemTemplates[rand_ind];
                var distractorPoint = new Vector3(
                    Random.Range(spawnArea.bounds.min.x, spawnArea.bounds.max.x),
                    Random.Range(spawnArea.bounds.min.y, spawnArea.bounds.max.y),
                    Random.Range(spawnArea.bounds.min.z, spawnArea.bounds.max.z)
                );

                // randomly pick an rotation
                var distractorRotation = Quaternion.Euler(
                    Random.Range(0, 360),
                    Random.Range(0, 360),
                    Random.Range(0, 360)
                );
                distractor = Instantiate(distractor, distractorPoint, distractorRotation);
                distractor.transform.parent = highlightItemController.transform;

                RandomSpawnUntilNoCollision(distractor);
            }
            
            // update list for the highlight controller
            highlightItemController.UpdateList(delayInSeconds);

            // define how we highlight
            switch (mode)
            {
                case Mode.SimilarName:
                    // use the first nameLength characters of the target name
                    highlightItemController.highlightName = target.name.Substring(0, nameLength).ToLower();
                    highlightItemController.currentHighlightMode = HighlightItemController.HighlightMode.Name;
                    break;
                case Mode.SimilarTags:
                    // Check what tags the target has
                    if (target.GetComponent<ItemTags>())
                    {
                        var tags = target.GetComponent<ItemTags>().itemTags;
                        // TODO pick some of the tags
                        highlightItemController.highlighted = tags;
                        highlightItemController.currentHighlightMode = HighlightItemController.HighlightMode.Tags;
                    }

                    break;
                case Mode.SimilarVolume:
                    // get volume from renderer bound
                    var renderer = target.GetComponent<Renderer>();
                    if (renderer)
                    {
                        var volume = renderer.bounds.size.x * renderer.bounds.size.y * renderer.bounds.size.z;
                        highlightItemController.volumeThresholdMin = volume - deltaVolume;
                        highlightItemController.volumeThresholdMax = volume + deltaVolume;
                        highlightItemController.currentHighlightMode = HighlightItemController.HighlightMode.Volume;
                    }

                    break;
            }

            isSpawning = false;

            // send information about target

            lslStreamer.SendEvent("Target generated: " + target.name);
            lslStreamer.SendEvent("Target location: " + target.transform.position.ToString("F4"));
        }

        if (mode == Mode.SimilarVolume && target)
        {
            // get volume from renderer bound
            var renderer = target.GetComponent<Renderer>();
            if (renderer)
            {
                var volume = renderer.bounds.size.x * renderer.bounds.size.y * renderer.bounds.size.z;
                highlightItemController.volumeThresholdMin = volume - deltaVolume;
                highlightItemController.volumeThresholdMax = volume + deltaVolume;
                highlightItemController.currentHighlightMode = HighlightItemController.HighlightMode.Volume;
            }
        }
    }
}
