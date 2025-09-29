using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using Object = UnityEngine.Object;

public class UniqueIDMenu
{
#if UNITY_EDITOR
    [MenuItem("Plugins/UniqueID/Generate UniqueID map")]
    static void GenerateUniqueIdMap()
    {
        UniqueIDMap[] objList = GameObject.FindObjectsOfType<UniqueIDMap>();
        string name = "UniqueIdMap";
        UniqueIDMap m = null;

        for (var i = 0; i < objList.Length; i++)
        {
            if (objList[i].gameObject.name == name)
            {
                m = objList[i];
                break;
            }
        }

        if (!m)
        {
            var gameObject = new GameObject();
            m = gameObject.AddComponent<UniqueIDMap>();
            m.name = name;
        }

        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject) o;
            if (g.GetComponent<UniqueID>())
            {
                var id = g.GetComponent<UniqueID>().guid;
                Debug.Log("Collect UniqueID " + id + " to: " + g.name);

                m.AddToMap(id, g);
            }
        }
    }

    [MenuItem("Plugins/UniqueID/Generate UniqueIDs for all gameObjects")]
    static void GenerateUniqueID()
    {
        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject) o;
            if (!g.GetComponent<UniqueID>())
            {
                UniqueID u = g.AddComponent<UniqueID>();
                if (string.IsNullOrEmpty(u.guid))
                {
                    Guid guid = Guid.NewGuid();
                    u.guid = g.name + "-" + guid.ToString();
                }

                Debug.Log("Add UniqueID " + u.guid + " to: " + g.name);
            }
        }
    }

    [MenuItem("Plugins/UniqueID/Generate UniqueIDs for selected gameObjects")]
    static void GenerateUniqueIDSelection()
    {
        foreach (object o in Selection.gameObjects)
        {
            GameObject g = (GameObject) o;
            if (!g.GetComponent<UniqueID>())
            {
                UniqueID u = g.AddComponent<UniqueID>();
                if (string.IsNullOrEmpty(u.guid))
                {
                    Guid guid = Guid.NewGuid();
                    u.guid = g.name + "-" + guid.ToString();
                }

                Debug.Log("Add UniqueID " + u.guid + " to: " + g.name);
            }
        }
    }

    [MenuItem("Plugins/UniqueID/Re-Generate UniqueIDs (Dangerous! This will break old replay!)")]
    static void ReGenerateUniqueID()
    {
        RemoveUniqueID();
        GenerateUniqueID();
    }

    [MenuItem("Plugins/UniqueID/Remove UniqueIDs (Dangerous! This will break old replay!)")]
    static void RemoveUniqueID()
    {
        object[] obj = GameObject.FindObjectsOfType(typeof(GameObject));
        foreach (object o in obj)
        {
            GameObject g = (GameObject) o;
            UniqueID u = g.GetComponent<UniqueID>();
            if (u)
            {
                Debug.Log("Remove UniqueID " + u.guid + " from: " + g.name);
                UnityEngine.Object.DestroyImmediate(u);
            }
        }
    }

    [MenuItem("Edit/Cleanup Missing Scripts")]
    static void CleanupMissingScripts ()
    {
        for(int i = 0; i < Selection.gameObjects.Length; i++)
        {
            var gameObject = Selection.gameObjects[i];
         
            // We must use the GetComponents array to actually detect missing components
            var components = gameObject.GetComponents<Component>();
         
            // Create a serialized object so that we can edit the component list
            var serializedObject = new SerializedObject(gameObject);
            // Find the component list property
            var prop = serializedObject.FindProperty("m_Component");
         
            // Track how many components we've removed
            int r = 0;
         
            // Iterate over all components
            for(int j = 0; j < components.Length; j++)
            {
                // Check if the ref is null
                if(components[j] == null)
                {
                    // If so, remove from the serialized component array
                    prop.DeleteArrayElementAtIndex(j-r);
                    // Increment removed count
                    r++;
                }
            }
         
            // Apply our changes to the game object
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif
}