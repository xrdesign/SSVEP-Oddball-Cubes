using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GradualMover : MonoBehaviour
{
    public float duration = 1.0f;
    public Vector3 position = Vector3.zero;

    private IEnumerator SmoothLerp(float time)
    {
        Vector3 startingPos = transform.localPosition;
        Vector3 finalPos = position;
        float elapsedTime = 0;

        while (elapsedTime < time)
        {
            transform.localPosition = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void Unlock()
    {
        StartCoroutine(SmoothLerp(duration));
    }

    private IEnumerator SmoothLerpAfter(float delay)
    {
        Vector3 startingPos = transform.localPosition;
        Vector3 finalPos = position;
        float elapsedTime = 0;

        while (elapsedTime - delay < duration)
        {
            if (elapsedTime > delay)
            {
                transform.localPosition = Vector3.Lerp(startingPos, finalPos, ((elapsedTime - delay) / duration));
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }

    public void UnlockAfter(float time)
    {
        StartCoroutine(SmoothLerpAfter(time));
    }


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
