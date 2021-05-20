using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorBehavior), true)]
public class DoorBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty opened;
    protected SerializedProperty locked;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        opened = serializedObject.FindProperty("opened");
        locked = serializedObject.FindProperty("locked");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Door params", HeaderStyle);

        EditorGUILayout.PropertyField(opened);
        EditorGUILayout.PropertyField(locked);

        serializedObject.ApplyModifiedProperties();
    }
}
