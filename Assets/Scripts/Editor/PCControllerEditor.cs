using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PCController))]
public class PCControllerEditor : Editor
{
    SerializedProperty inventoryGO;
    SerializedProperty thinkSpotLight;
    SerializedProperty newScene;
    SerializedProperty location;

    SerializedProperty MovementController;
    SerializedProperty ActionController;
    SerializedProperty AnimationController;
    SerializedProperty InventoryController;

    SerializedProperty pickClip;
    SerializedProperty footstepClips;

    SerializedProperty chairSittingClip;
    SerializedProperty chairStandUpClip;
    SerializedProperty couchSittingClip;
    SerializedProperty couchStandUpClip;

    GUIStyle headerStyle;

    public static bool actionVerbsFoldout;
    public static bool audioVariablesFoldout;

    private void OnEnable()
    {
        inventoryGO = serializedObject.FindProperty("inventoryGO");
        thinkSpotLight = serializedObject.FindProperty("thinkSpotLight");
        newScene = serializedObject.FindProperty("newScene");
        location = serializedObject.FindProperty("location");

        MovementController = serializedObject.FindProperty("MovementController");
        ActionController = serializedObject.FindProperty("ActionController");
        AnimationController = serializedObject.FindProperty("AnimationController");
        InventoryController = serializedObject.FindProperty("InventoryController");

        pickClip = serializedObject.FindProperty("pickClip");
        footstepClips = serializedObject.FindProperty("footstepClips");

        chairSittingClip = serializedObject.FindProperty("chairSittingClip");
        chairStandUpClip = serializedObject.FindProperty("chairStandUpClip");
        couchSittingClip = serializedObject.FindProperty("couchSittingClip");
        couchStandUpClip = serializedObject.FindProperty("couchStandUpClip");

        headerStyle = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("PCController", headerStyle);

        EditorGUILayout.PropertyField(inventoryGO);
        EditorGUILayout.PropertyField(thinkSpotLight);
        EditorGUILayout.PropertyField(newScene);
        EditorGUILayout.PropertyField(location);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Movement Controller", headerStyle);

        EditorGUILayout.PropertyField(MovementController);

        if(MovementController != null && MovementController.objectReferenceValue != null)
        {
            Editor MovementControllerEditor = CreateEditor(MovementController.objectReferenceValue);
            MovementControllerEditor.OnInspectorGUI();
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Action Controller", headerStyle);

        EditorGUILayout.PropertyField(ActionController);

        if( ActionController != null && ActionController.objectReferenceValue != null)
        {
            actionVerbsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(actionVerbsFoldout, "Action verbs");

            if (actionVerbsFoldout)
            {
                Editor ActionControllerEditor = CreateEditor(ActionController.objectReferenceValue);
                ActionControllerEditor.OnInspectorGUI();
            }

            EditorGUILayout.EndFoldoutHeaderGroup();
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Animation Controller", headerStyle);

        EditorGUILayout.PropertyField(AnimationController);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Inventory Controller", headerStyle);

        EditorGUILayout.PropertyField(InventoryController);

        EditorGUILayout.Space(15);

        audioVariablesFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(audioVariablesFoldout, "Audio variables");

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (audioVariablesFoldout)
        {
            EditorGUILayout.PropertyField(pickClip);
            EditorGUILayout.PropertyField(footstepClips);
            EditorGUILayout.PropertyField(chairSittingClip);
            EditorGUILayout.PropertyField(chairStandUpClip);
            EditorGUILayout.PropertyField(couchSittingClip);
            EditorGUILayout.PropertyField(couchStandUpClip);
        }

        serializedObject.ApplyModifiedProperties();
    }
}

public class EditorTools
{

    static List<string> layers;
    static string[] layerNames;

    public static LayerMask LayerMaskField(string label, LayerMask selected)
    {

        if (layers == null)
        {
            layers = new List<string>();
            layerNames = new string[4];
        }
        else
        {
            layers.Clear();
        }

        int emptyLayers = 0;
        for (int i = 0; i < 32; i++)
        {
            string layerName = LayerMask.LayerToName(i);

            if (layerName != "")
            {

                for (; emptyLayers > 0; emptyLayers--) layers.Add("Layer " + (i - emptyLayers));
                layers.Add(layerName);
            }
            else
            {
                emptyLayers++;
            }
        }

        if (layerNames.Length != layers.Count)
        {
            layerNames = new string[layers.Count];
        }
        for (int i = 0; i < layerNames.Length; i++) layerNames[i] = layers[i];

        selected.value = EditorGUILayout.MaskField(label, selected.value, layerNames);

        return selected;
    }
}
