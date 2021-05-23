using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCBehavior), true), CanEditMultipleObjects]
public class NPCControllerEditor : Editor
{
    protected SerializedProperty MovementController;
    protected SerializedProperty AnimationController;

    protected SerializedProperty obj;

    GUIStyle headerStyle;

    private void OnEnable()
    {
        MovementController = serializedObject.FindProperty("MovementController");
        AnimationController = serializedObject.FindProperty("AnimationController");

        obj = serializedObject.FindProperty("obj");

        headerStyle = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        serializedObject.Update();

        EditorGUILayout.LabelField("Movement Controller", headerStyle);

        EditorGUILayout.PropertyField(MovementController);

        if (MovementController != null && MovementController.objectReferenceValue != null)
        {
            Editor MovementControllerEditor = CreateEditor(MovementController.objectReferenceValue);
            MovementControllerEditor.OnInspectorGUI();
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Animation Controller", headerStyle);

        EditorGUILayout.PropertyField(AnimationController);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(obj);

        if (obj != null && obj.objectReferenceValue != null)
        {
            InteractableObjEditor editor = (InteractableObjEditor)CreateEditor(obj.objectReferenceValue);

            editor.serializedObject.Update();

            editor.ObjectGUI();

            editor.serializedObject.ApplyModifiedProperties();
        }

        serializedObject.ApplyModifiedProperties();
    }
}
