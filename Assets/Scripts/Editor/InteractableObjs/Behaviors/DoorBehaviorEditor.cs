using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DoorBehavior), true), CanEditMultipleObjects]
public class DoorBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty opened;
    protected SerializedProperty locked;

    protected SerializedProperty openClip;
    protected SerializedProperty closeClip;

    protected SerializedProperty lockedComment;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        opened = serializedObject.FindProperty("opened");
        locked = serializedObject.FindProperty("locked");

        openClip = serializedObject.FindProperty("openClip");
        closeClip = serializedObject.FindProperty("closeClip");

        lockedComment = serializedObject.FindProperty("lockedComment");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Door params", HeaderStyle);

        EditorGUILayout.PropertyField(opened);
        EditorGUILayout.PropertyField(locked);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(openClip);
        EditorGUILayout.PropertyField(closeClip);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(lockedComment);

        serializedObject.ApplyModifiedProperties();
    }
}
