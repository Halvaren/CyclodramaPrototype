using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PickableObjBehavior), true)]
public class PickableObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty useReactions;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        useReactions = serializedObject.FindProperty("useReactions");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        UseReactionsGUI();

        serializedObject.ApplyModifiedProperties();
    }

    void UseReactionsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Reactions to verb Use", headerStyle);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+"))
        {
            useReactions.arraySize++;
        }

        EditorGUILayout.EndHorizontal(); 
        
        if (useReactions.isArray)
        {
            int elementCount = useReactions.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty useReaction = useReactions.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    useReactions.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                UseReactionGUI(useReaction, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void UseReactionGUI(SerializedProperty property, int i)
    {
        SerializedProperty index = property.FindPropertyRelative("index");
        SerializedProperty objs = property.FindPropertyRelative("objs");

        SerializedProperty objSet = property.FindPropertyRelative("objSet");

        EditorGUILayout.PropertyField(index);
        EditorGUILayout.PropertyField(objSet);

        if((UseReactionObjSet)objSet.enumValueIndex == UseReactionObjSet.ListOfObjs)
            EditorGUILayout.PropertyField(objs);
    }
}
