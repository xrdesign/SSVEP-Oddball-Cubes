using UnityEditor;
using UnityEngine;
using System;

#if UNITY_EDITOR
// Place this file inside Assets/Editor
[CustomPropertyDrawer(typeof(UniqueIdentifierAttribute))]
public class UniqueIdDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty prop, GUIContent label)
    {
        // Place a label so it can't be edited by accident
        Rect textFieldPosition = position;
        textFieldPosition.height = 16;
        EditorGUI.LabelField(textFieldPosition, label, new GUIContent(prop.stringValue));
    }
}
#endif