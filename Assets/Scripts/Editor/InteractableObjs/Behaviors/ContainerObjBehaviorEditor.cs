using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ContainerObjBehavior))]
public class ContainerObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty detailCameraBehavior;
    protected SerializedProperty objBehaviors;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        detailCameraBehavior = serializedObject.FindProperty("detailCameraBehavior");
        objBehaviors = serializedObject.FindProperty("objBehaviors");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(detailCameraBehavior);
        EditorGUILayout.PropertyField(objBehaviors);

        serializedObject.ApplyModifiedProperties();
    }
}
