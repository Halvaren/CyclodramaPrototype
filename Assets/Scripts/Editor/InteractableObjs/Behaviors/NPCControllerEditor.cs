using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCBehavior), true), CanEditMultipleObjects]
public class NPCControllerEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty MovementController;

    protected SerializedProperty defaultGiveAnswer;
    protected SerializedProperty defaultConvinceAnswer;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        MovementController = serializedObject.FindProperty("MovementController");
        defaultGiveAnswer = serializedObject.FindProperty("defaultGiveAnswer");
        defaultConvinceAnswer = serializedObject.FindProperty("defaultConvinceAnswer");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Movement Controller", headerStyle);

        EditorGUILayout.PropertyField(MovementController);

        if (MovementController != null && MovementController.objectReferenceValue != null)
        {
            Editor MovementControllerEditor = CreateEditor(MovementController.objectReferenceValue);
            MovementControllerEditor.OnInspectorGUI();
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(defaultGiveAnswer);
        EditorGUILayout.PropertyField(defaultConvinceAnswer);

        serializedObject.ApplyModifiedProperties();
    }
}
