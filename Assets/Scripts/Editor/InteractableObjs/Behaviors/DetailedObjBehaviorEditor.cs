using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(DetailedObjBehavior), true), CanEditMultipleObjects]
public class DetailedObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty detailCameraBehavior;
    protected SerializedProperty detailedObjGO;
    protected SerializedProperty detailedLight;

    protected SerializedProperty lightReductionMultiplier;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        detailCameraBehavior = serializedObject.FindProperty("detailCameraBehavior");
        detailedObjGO = serializedObject.FindProperty("detailedObjGO");
        detailedLight = serializedObject.FindProperty("detailedLight");

        lightReductionMultiplier = serializedObject.FindProperty("lightReductionMultiplier");
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

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(lightReductionMultiplier);

        serializedObject.ApplyModifiedProperties();
    }
}

[CustomEditor(typeof(DetailedEmitterObjBehavior), true), CanEditMultipleObjects]
public class DetailedEmitterObjBehaviorEditor : InteractableObjBehaviorEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(objWeight);
        EditorGUILayout.PropertyField(objHeight);
        EditorGUILayout.PropertyField(characterVisibleToPick);

        serializedObject.ApplyModifiedProperties();
    }
}
