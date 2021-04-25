using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(EmitterObjBehavior), true)]
public class EmitterObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty dropObjs;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        dropObjs = serializedObject.FindProperty("dropObjs");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        DropObjsGUI();

        serializedObject.ApplyModifiedProperties();
    }

    void DropObjsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Drop objects", headerStyle);

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
