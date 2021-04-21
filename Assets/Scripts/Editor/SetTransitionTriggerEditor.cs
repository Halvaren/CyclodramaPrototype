using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SetTransitionTrigger))]
public class SetTransitionTriggerEditor : Editor
{
    string[] dropdownOptions = { "Lineal Movement", "Wait at Point", "Follow Waypoints" };

    SerializedProperty setTransitionMovement;

    SerializedProperty rotation;

    SerializedProperty distanceBetweenSets;
    SerializedProperty offset;

    SerializedProperty currentSet;
    SerializedProperty nextSet;

    SerializedProperty connectionIndex;
    SerializedProperty nextSetName;

    SerializedProperty characterTransitionMovement;

    SerializedProperty characterXMovement;
    SerializedProperty characterYMovement;
    SerializedProperty characterZMovement;

    SerializedProperty characterWaitPosition;

    SerializedProperty waypointsInNextTrigger;
    SerializedProperty characterWaypoints;

    SerializedProperty characterWaitsUntilSetMovementIsDone;

    SerializedProperty characterFinalPosition;

    GUIStyle headerStyle;

    void OnEnable()
    {
        currentSet = serializedObject.FindProperty("currentSet");
        nextSet = serializedObject.FindProperty("nextSet");

        connectionIndex = serializedObject.FindProperty("connectionIndex");
        nextSetName = serializedObject.FindProperty("nextSetName");

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

        headerStyle = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold};
        headerStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("Connection params", headerStyle);

        EditorGUILayout.PropertyField(currentSet);
        EditorGUILayout.PropertyField(nextSet);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(connectionIndex);
        EditorGUILayout.PropertyField(nextSetName);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Set movement/rotation params", headerStyle);

        EditorGUILayout.PropertyField(setTransitionMovement);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(rotation);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(distanceBetweenSets);
        EditorGUILayout.PropertyField(offset);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Playable character movement params", headerStyle);

        characterTransitionMovement.intValue = EditorGUILayout.Popup(characterTransitionMovement.intValue, dropdownOptions);

        switch(characterTransitionMovement.intValue)
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

        if(!waypointsInNextTrigger.boolValue)
            EditorGUILayout.PropertyField(characterWaypoints);
    }
}
