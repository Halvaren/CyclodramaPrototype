using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EmployeeDoorBehavior)), CanEditMultipleObjects]
public class EmployeeDoorBehaviorEditor : DoorBehaviorEditor
{
    protected SerializedProperty questsNotCompletedYet;
    protected SerializedProperty cantUnlockComment;
    protected SerializedProperty doorTrigger;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        questsNotCompletedYet = serializedObject.FindProperty("questsNotCompletedYet");
        cantUnlockComment = serializedObject.FindProperty("cantUnlockComment");
        doorTrigger = serializedObject.FindProperty("doorTrigger");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(questsNotCompletedYet);
        EditorGUILayout.PropertyField(cantUnlockComment);
        EditorGUILayout.PropertyField(doorTrigger);

        serializedObject.ApplyModifiedProperties();
    }
}
