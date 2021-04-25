using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ContainerObjBehavior), true)]
public class ContainerObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty accessible;
    protected SerializedProperty detailCameraBehavior;
    protected SerializedProperty detailLighting;
    protected SerializedProperty objBehaviors;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        accessible = serializedObject.FindProperty("accessible");
        detailCameraBehavior = serializedObject.FindProperty("detailCameraBehavior");
        detailLighting = serializedObject.FindProperty("detailLighting");
        objBehaviors = serializedObject.FindProperty("objBehaviors");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(accessible);
        EditorGUILayout.PropertyField(detailCameraBehavior);
        EditorGUILayout.PropertyField(detailLighting);
        EditorGUILayout.PropertyField(objBehaviors);

        serializedObject.ApplyModifiedProperties();
    }
}
