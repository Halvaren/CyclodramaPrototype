using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NPCBehavior), true), CanEditMultipleObjects]
public class NPCBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty MovementController;

    protected SerializedProperty defaultGiveAnswer;
    protected SerializedProperty defaultConvinceAnswer;
    protected SerializedProperty location;
    protected SerializedProperty firstTimeTalk;

    protected SerializedProperty footstepClips;

    protected SerializedProperty chairSittingClip;
    protected SerializedProperty chairStandUpClip;
    protected SerializedProperty couchSittingClip;
    protected SerializedProperty couchStandUpClip;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        MovementController = serializedObject.FindProperty("MovementController");
        defaultGiveAnswer = serializedObject.FindProperty("defaultGiveAnswer");
        defaultConvinceAnswer = serializedObject.FindProperty("defaultConvinceAnswer");
        location = serializedObject.FindProperty("location");
        firstTimeTalk = serializedObject.FindProperty("firstTimeTalk");

        footstepClips = serializedObject.FindProperty("footstepClips");

        chairSittingClip = serializedObject.FindProperty("chairSittingClip");
        chairStandUpClip = serializedObject.FindProperty("chairStandUpClip");
        couchSittingClip = serializedObject.FindProperty("couchSittingClip");
        couchStandUpClip = serializedObject.FindProperty("couchStandUpClip");
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

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(location);
        EditorGUILayout.PropertyField(firstTimeTalk);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(footstepClips);
        EditorGUILayout.PropertyField(chairSittingClip);
        EditorGUILayout.PropertyField(chairStandUpClip);
        EditorGUILayout.PropertyField(couchSittingClip);
        EditorGUILayout.PropertyField(couchStandUpClip);

        serializedObject.ApplyModifiedProperties();
    }
}
