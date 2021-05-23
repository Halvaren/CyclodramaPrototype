using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EmitterObjBehavior), true), CanEditMultipleObjects]
public class EmitterObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty dropObjs;

    protected SerializedProperty emptyComment;
    protected SerializedProperty haveEnoughComment;
    protected SerializedProperty dropObjsComment;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        dropObjs = serializedObject.FindProperty("dropObjs");

        emptyComment = serializedObject.FindProperty("emptyComment");
        haveEnoughComment = serializedObject.FindProperty("haveEnoughComment");
        dropObjsComment = serializedObject.FindProperty("dropObjsComment");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(emptyComment);
        EditorGUILayout.PropertyField(haveEnoughComment);
        EditorGUILayout.PropertyField(dropObjsComment);

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(objWeight);
        EditorGUILayout.PropertyField(objHeight);
        EditorGUILayout.PropertyField(characterVisibleToPick);

        DropObjsGUI();

        serializedObject.ApplyModifiedProperties();
    }

    void DropObjsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Drop objects", HeaderStyle);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+"))
        {
            dropObjs.arraySize++;
        }

        EditorGUILayout.EndHorizontal();

        if (dropObjs.isArray)
        {
            int elementCount = dropObjs.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty useReaction = dropObjs.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    dropObjs.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                DropObjGUI(useReaction, i);

                EditorGUILayout.Space(15);
            }
        }

        void DropObjGUI(SerializedProperty property, int i)
        {
            SerializedProperty obj = property.FindPropertyRelative("obj");
            SerializedProperty quantity = property.FindPropertyRelative("quantity");
            SerializedProperty banObjs = property.FindPropertyRelative("banObjs");

            EditorGUILayout.PropertyField(obj);
            EditorGUILayout.PropertyField(quantity);
            EditorGUILayout.PropertyField(banObjs);   
        }
    }
}
