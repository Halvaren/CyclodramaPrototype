using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DetailedObjBehavior), true)]
public class DetailedObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty detailCameraBehavior;
    protected SerializedProperty detailedObjGO;
    protected SerializedProperty detailedLight;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        detailCameraBehavior = serializedObject.FindProperty("detailCameraBehavior");
        detailedObjGO = serializedObject.FindProperty("detailedObjGO");
        detailedLight = serializedObject.FindProperty("detailedLight");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(detailCameraBehavior);
        EditorGUILayout.PropertyField(detailedObjGO);
        EditorGUILayout.PropertyField(detailedLight);

        serializedObject.ApplyModifiedProperties();
    }
}
