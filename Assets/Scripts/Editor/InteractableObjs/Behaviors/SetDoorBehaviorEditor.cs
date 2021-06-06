using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetDoorBehavior), true), CanEditMultipleObjects]
public class SetDoorBehaviorEditor : DoorBehaviorEditor
{
    string[] dropdownOptions = { "Lineal Movement", "Wait at Point", "Follow Waypoints" };

    protected SerializedProperty transitionTrigger;
    protected SerializedProperty doorSign;

    protected SerializedProperty requiresGear;
    protected SerializedProperty cantUnlockComment;

    #region SetTransitionTrigger

    protected SerializedProperty setTransitionMovement;

    protected SerializedProperty rotation;

    protected SerializedProperty distanceBetweenSets;
    protected SerializedProperty offset;

    protected SerializedProperty nextSet;

    protected SerializedProperty connectionIndex;

    protected SerializedProperty characterTransitionMovement;

    protected SerializedProperty characterXMovement;
    protected SerializedProperty characterYMovement;
    protected SerializedProperty characterZMovement;

    protected SerializedProperty characterWaitPosition;

    protected SerializedProperty waypointsInNextTrigger;
    protected SerializedProperty characterWaypoints;

    protected SerializedProperty characterWaitsUntilSetMovementIsDone;

    protected SerializedProperty characterFinalPosition;

    #endregion

    public static bool connectionParamsFoldout = true;
    public static bool setMoveRotParamsFoldout = true;
    public static bool PCMoveParamsFoldout = true;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        transitionTrigger = serializedObject.FindProperty("transitionTrigger");
        doorSign = serializedObject.FindProperty("doorSign");

        requiresGear = serializedObject.FindProperty("requiresGear");
        cantUnlockComment = serializedObject.FindProperty("cantUnlockComment");

        currentSet = serializedObject.FindProperty("currentSet");
        nextSet = serializedObject.FindProperty("nextSet");

        connectionIndex = serializedObject.FindProperty("connectionIndex");

        setTransitionMovement = serializedObject.FindProperty("setTransitionMovement");

        distanceBetweenSets = serializedObject.FindProperty("distanceBetweenSets");
        offset = serializedObject.FindProperty("offset");

        rotation = serializedObject.FindProperty("rotation");

        characterTransitionMovement = serializedObject.FindProperty("characterTransitionMovement");

        characterXMovement = serializedObject.FindProperty("characterXMovement");
        characterYMovement = serializedObject.FindProperty("characterYMovement");
        characterZMovement = serializedObject.FindProperty("characterZMovement");

        characterWaitPosition = serializedObject.FindProperty("characterWaitPosition");

        waypointsInNextTrigger = serializedObject.FindProperty("waypointsInNextTrigger");
        characterWaypoints = serializedObject.FindProperty("characterWaypoints");

        characterWaitsUntilSetMovementIsDone = serializedObject.FindProperty("characterWaitsUntilSetMovementIsDone");

        characterFinalPosition = serializedObject.FindProperty("characterFinalPosition");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        serializedObject.Update();

        base.OnInspectorGUI();
        EditorGUILayout.PropertyField(transitionTrigger);
        EditorGUILayout.PropertyField(doorSign);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(requiresGear);
        EditorGUILayout.PropertyField(cantUnlockComment);

        EditorGUILayout.Space(15);

        connectionParamsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(connectionParamsFoldout, "Connection params");

        if(connectionParamsFoldout)
        {
            EditorGUILayout.PropertyField(currentSet);
            EditorGUILayout.PropertyField(nextSet);

            EditorGUILayout.Space(15);

            EditorGUILayout.PropertyField(connectionIndex);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(15);

        setMoveRotParamsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(setMoveRotParamsFoldout, "Set movement/rotation params");

        if(setMoveRotParamsFoldout)
        {
            EditorGUILayout.PropertyField(setTransitionMovement);

            EditorGUILayout.Space(15);

            EditorGUILayout.PropertyField(rotation);

            EditorGUILayout.Space(15);

            EditorGUILayout.PropertyField(distanceBetweenSets);
            EditorGUILayout.PropertyField(offset);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(15);

        PCMoveParamsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(PCMoveParamsFoldout, "Playable character movement params");

        if(PCMoveParamsFoldout)
        {
            characterTransitionMovement.intValue = EditorGUILayout.Popup(characterTransitionMovement.intValue, dropdownOptions);

            switch (characterTransitionMovement.intValue)
            {
                case 0:
                    LinealMovementGUI();
                    break;
                case 1:
                    WaitAtPointGUI();
                    break;
                case 2:
                    FollowWaypointsGUI();
                    break;
            }

            EditorGUILayout.Space(15);

            EditorGUILayout.PropertyField(characterFinalPosition);
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        serializedObject.ApplyModifiedProperties();
    }

    void LinealMovementGUI()
    {
        EditorGUILayout.PropertyField(characterXMovement);
        EditorGUILayout.PropertyField(characterYMovement);
        EditorGUILayout.PropertyField(characterZMovement);

        EditorGUILayout.PropertyField(characterWaitsUntilSetMovementIsDone, new GUIContent("Waits until set mov. is done"));
    }

    void WaitAtPointGUI()
    {
        EditorGUILayout.PropertyField(characterWaitPosition);
    }

    void FollowWaypointsGUI()
    {
        EditorGUILayout.PropertyField(waypointsInNextTrigger);

        if (!waypointsInNextTrigger.boolValue)
            EditorGUILayout.PropertyField(characterWaypoints);
    }
}
