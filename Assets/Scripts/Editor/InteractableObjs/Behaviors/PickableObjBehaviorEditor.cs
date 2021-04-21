using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PickableObjBehavior))]
public class PickableObjBehaviorEditor : InteractableObjBehaviorEditor
{
    //protected SerializedProperty inInventory;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        //inInventory = serializedObject.FindProperty("inInventory");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        serializedObject.Update();

        //EditorGUILayout.Space(15);

        //EditorGUILayout.PropertyField(inInventory);

        serializedObject.ApplyModifiedProperties();
    }
}
