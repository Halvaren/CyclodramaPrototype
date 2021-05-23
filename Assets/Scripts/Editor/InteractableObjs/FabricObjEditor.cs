using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(FabricObj))]
public class FabricObjEditor : InteractableObjEditor
{
    protected SerializedProperty realName;

    protected SerializedProperty redSprite;
    protected SerializedProperty pinkSprite;
    protected SerializedProperty purpleSprite;
    protected SerializedProperty navyBlueSprite;
    protected SerializedProperty lightBlueSprite;
    protected SerializedProperty greenSprite;
    protected SerializedProperty greenishYellowSprite;
    protected SerializedProperty yellowSprite;
    protected SerializedProperty orangeSprite;
    protected SerializedProperty whiteSprite;
    protected SerializedProperty blackSprite;
    protected SerializedProperty greySprite;

    protected static bool colorsFoldout = true;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        realName = serializedObject.FindProperty("realName");

        redSprite = serializedObject.FindProperty("redSprite");
        pinkSprite = serializedObject.FindProperty("pinkSprite");
        purpleSprite = serializedObject.FindProperty("purpleSprite");
        navyBlueSprite = serializedObject.FindProperty("navyBlueSprite");
        lightBlueSprite = serializedObject.FindProperty("lightBlueSprite");
        greenSprite = serializedObject.FindProperty("greenSprite");
        greenishYellowSprite = serializedObject.FindProperty("greenishYellowSprite");
        yellowSprite = serializedObject.FindProperty("yellowSprite");
        orangeSprite = serializedObject.FindProperty("orangeSprite");
        whiteSprite = serializedObject.FindProperty("whiteSprite");
        blackSprite = serializedObject.FindProperty("blackSprite");
        greySprite = serializedObject.FindProperty("greySprite");
    }

    public override void ObjectGUI()
    {
        base.ObjectGUI();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(realName);

        colorsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(colorsFoldout, "Colored sprites");

        EditorGUILayout.EndFoldoutHeaderGroup();

        if(colorsFoldout)
        {
            EditorGUILayout.PropertyField(redSprite);
            EditorGUILayout.PropertyField(pinkSprite);
            EditorGUILayout.PropertyField(purpleSprite);
            EditorGUILayout.PropertyField(navyBlueSprite);
            EditorGUILayout.PropertyField(lightBlueSprite);
            EditorGUILayout.PropertyField(greenSprite);
            EditorGUILayout.PropertyField(greenishYellowSprite);
            EditorGUILayout.PropertyField(yellowSprite);
            EditorGUILayout.PropertyField(orangeSprite);
            EditorGUILayout.PropertyField(whiteSprite);
            EditorGUILayout.PropertyField(blackSprite);
            EditorGUILayout.PropertyField(greySprite);
        }
    }
}