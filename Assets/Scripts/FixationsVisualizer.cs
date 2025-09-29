using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fixation
{
    public float startingTime;
    public float duration;
    public Vector3 position;
}

public class FixationsVisualizer : MonoBehaviour
{
    
    public TextAsset File;
    public GameObject FixationPrefab;
    public GameObject List;
    public Vector3 scale = Vector3.one;

    private List<Fixation> fixations;
    private List<GameObject> fixationObjects;
    private float maxDuration = -1;
    private float secMaxDuration = -1;
    private float minDuration = float.MaxValue;
    private bool isEnabled = false;

    private float colorScale = 1;

    void Start()
    {
        fixations = new List<Fixation>();
        fixationObjects = new List<GameObject>();
        ParseFile();
        RenderFixations();
    }
	
	void Update ()
    {
		
	}

    void ParseFile ()
    {
        string content = File.ToString();
        string[] lines = content.Split('\n');

        // Skip line one metadata
        for (int i = 1; i < lines.Length; i++)
        {
            string[] tokens = lines[i].Split(',');
            if (tokens.Length < 5)
            {
                continue;
            }
            Fixation fix = new Fixation();
            fix.startingTime = float.Parse(tokens[0].Replace(" ", ""));
            fix.duration = float.Parse(tokens[1].Replace(" ", ""));
            fix.position = new Vector3(float.Parse(tokens[2].Replace(" ", "")) * scale.x, float.Parse(tokens[3].Replace(" ", "")) * scale.y, float.Parse(tokens[4].Replace(" ", "")) * scale.z);
           
            fixations.Add(fix);

            if (fix.duration > maxDuration)
            {
                secMaxDuration = maxDuration;
                maxDuration = fix.duration;
            }

            if (fix.duration < minDuration)
            {
                minDuration = fix.duration;
            }
        }

        colorScale = 1.0f / (secMaxDuration - minDuration);
    }

    void RenderFixations ()
    {
        foreach (var fix in fixations)
        {
            GameObject go = Instantiate(FixationPrefab, fix.position, Quaternion.identity);
            MeshRenderer mr = go.GetComponent<MeshRenderer>();
            Material mat = Instantiate(mr.material);

            // color based on duration
            float col = (fix.duration - minDuration) * colorScale * 0.5f + 0.5f;
            mat.color = new Color(col, col, 0, col);
            mr.material = mat;
            go.transform.parent = List.transform;
            fixationObjects.Add(go);
        }
    }

    
}