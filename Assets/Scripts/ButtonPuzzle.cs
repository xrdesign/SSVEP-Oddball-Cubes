using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonPuzzle : MonoBehaviour
{
    public List<int> correctAnswer = new List<int> {3, 4, 5};
    public GameObject table;
    public AudioSource source;
    public EscapeRoom_PuzzleManager puzzleManager;
    private List<int> currentAnswer = new List<int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Push(int number)
    {
        while (currentAnswer.Count >= 3)
        {
            currentAnswer.RemoveAt(0);
        }
        currentAnswer.Add(number);

        if (puzzleManager)
        {
            puzzleManager.SendButton(number);
        }

        CheckComplete();
    }

    bool CheckMatch(List<int> l1, List<int> l2)
    {
        for (int i = 0; i < l1.Count; i++)
        {
            if (l1[i] != l2[i])
                return false;
        }
        return true;
    }

    void CheckComplete()
    {
        if (currentAnswer.Count == 3)
        {
            if (CheckMatch(correctAnswer, currentAnswer))
            {
                // Answer is correct, Show table
                if (table)
                {
                    table.SetActive(true);
                }

                if (source)
                {
                    source.Play();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
