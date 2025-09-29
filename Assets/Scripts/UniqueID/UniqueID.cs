using System;
using UnityEngine;
using System.Collections;

// Placeholder for UniqueIdDrawer script
public class UniqueIdentifierAttribute : PropertyAttribute
{
}

[Serializable]
public class UniqueID : MonoBehaviour
{
    [UniqueIdentifier,SerializeField] public string guid = "";
}