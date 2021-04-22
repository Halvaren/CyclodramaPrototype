using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PCController))]
public class PCControllerEditor : Editor
{
    SerializedProperty inventoryGO;

    SerializedProperty MovementController;
    SerializedProperty InputController;
    SerializedProperty ActionController;
    SerializedProperty AnimationController;
    SerializedProperty InventoryController;

    GUIStyle headerStyle;

    public bool actionVerbsFoldout;

    private void OnEnable()
    {
        inventoryGO = serializedObject.FindProperty("inventoryGO");

        MovementController = serializedObject.FindProperty("MovementController");
        InputController = serializedObject.FindProperty("InputController");
        ActionController = serializedObject.FindProperty("ActionController");
        AnimationController = serializedObject.FindProperty("AnimationController");
        InventoryController = serializedObject.FindProperty("InventoryController");

        headerStyle = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.LabelField("PCController", headerStyle);

        EditorGUILayout.PropertyField(inventoryGO);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Movement Controller", headerStyle);

        EditorGUILayout.PropertyField(MovementController);

        if(MovementController != null && MovementController.objectReferenceValue != null)
        {
            Editor MovementControllerEditor = CreateEditor(MovementController.objectReferenceValue);
            MovementControllerEditor.OnInspectorGUI();
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Input Controller", headerStyle);

        EditorGUILayout.PropertyField(InputController);

        if (InputController != null && InputController.objectReferenceValue != null)
        {
            Editor InputControllerEditor = CreateEditor(InputController.objectReferenceValue);
            InputControllerEditor.OnInspectorGUI();
        }

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Action Controller", headerStyle);

        EditorGUILayout.PropertyField(ActionController);

        actionVerbsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(actionVerbsFoldout, "Action verbs");

        if(actionVerbsFoldout)
        {
            if (ActionController != null && ActionController.objectReferenceValue != null)
            {
                Editor ActionControllerEditor = CreateEditor(ActionController.objectReferenceValue);
                ActionControllerEditor.OnInspectorGUI();
            }
        }

        EditorGUILayout.EndFoldoutHeaderGroup();

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Animation Controller", headerStyle);

        EditorGUILayout.PropertyField(AnimationController);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Inventory Controller", headerStyle);

        EditorGUILayout.PropertyField(InventoryController);

        EditorGUILayout.Space(15);

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
