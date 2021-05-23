using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractableObj))]
public class InteractableObjEditor : Editor
{
    protected InteractableObj obj;

    protected SerializedProperty ID;
    protected new SerializedProperty name;
    protected SerializedProperty inventorySprite;

    protected GUIStyle titleStyle;

    protected void OnEnable()
    {
        InitializeEditor();
    }

    protected virtual void InitializeEditor()
    {
        if (target != null)
            obj = (InteractableObj)target;

        ID = serializedObject.FindProperty("objID");
        name = serializedObject.FindProperty("name");
        inventorySprite = serializedObject.FindProperty("inventorySprite");

        titleStyle = new GUIStyle() { fontSize = 17, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
        titleStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if(obj != null)
        {
            if (string.IsNullOrEmpty(obj.name))
                EditorGUILayout.LabelField("New object", titleStyle);
            else
                EditorGUILayout.LabelField(obj.name, titleStyle);

            ObjectGUI();
        }        

        serializedObject.ApplyModifiedProperties();
    }

    public virtual void ObjectGUI()
    {
        EditorGUILayout.PropertyField(ID);

        EditorGUILayout.PropertyField(name);

        inventorySprite.objectReferenceValue = (Sprite)EditorGUILayout.ObjectField(new GUIContent("Inventory Sprite"), inventorySprite.objectReferenceValue, typeof(Sprite), false);
    }
}
