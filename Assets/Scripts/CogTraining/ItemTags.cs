using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Flags]
public enum MaskedEnum
{
    IsOffice = (1 << 0),
    IsCan = (1 << 1),
    IsBottle = (1 << 2),
    IsAlcohol = (1 << 3),
    IsPaper = (1 << 4),
    IsDrink = (1 << 5),
    IsNTP = (1 << 6),
}

public class ItemTags : MonoBehaviour
{
    //[BitMask(typeof(MaskedEnum))]
    public MaskedEnum itemTags;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
