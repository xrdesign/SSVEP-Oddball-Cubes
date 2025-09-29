using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class UniqueIDMap : MonoBehaviour {

    [System.Serializable]
    public class ObjectIdPair
    {
        public string guid;
        public GameObject gameObject;
    }

    void OnValidate()
    {
        result.Clear();
        if (search == "")
        {
            return;
        }

        IEnumerable<string> fullMatchingKeys = 
            dictionary.Keys.Where(currentKey => currentKey.Contains(search));

        int count = 0;
        foreach (string currentKey in fullMatchingKeys)
        {
            if (count >= resultLimit)
            {
                break;
            }
            result.Add(dictionary[currentKey]);
            count++;
        }
        
    }

    public int resultLimit = 20;
    public string search = "";
    public List<ObjectIdPair> result = new List<ObjectIdPair>();

    private List<ObjectIdPair> map = new List<ObjectIdPair>();
    private Dictionary<string, ObjectIdPair> dictionary = new Dictionary<string, ObjectIdPair>();

    public bool AddToMap(string guid, GameObject gameObject)
    {
        if (dictionary.ContainsKey(guid))
        {
            return false;
        }
        else
        {
            // Add to map
            var pair = new ObjectIdPair();
            pair.guid = guid;
            pair.gameObject = gameObject;
            map.Add(pair);
            dictionary.Add(guid, pair);
            return true;
        }
    }

    public GameObject FindGameObject(string guid)
    {
        ObjectIdPair g = null;
        dictionary.TryGetValue(guid, out g);
        return g.gameObject;
    }
}
