using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractableObjBehavior))]
public class InteractableObjBehaviorEditor : Editor
{
    protected InteractableObjBehavior behavior;

    protected SerializedProperty obj;
    protected SerializedProperty verbs;
    protected SerializedProperty triggerCollider;

    protected GUIStyle headerStyle;

    protected void OnEnable()
    {
        InitializeEditor();
    }

    protected virtual void InitializeEditor()
    {
        behavior = (InteractableObjBehavior)target;

        obj = serializedObject.FindProperty("obj");
        verbs = serializedObject.FindProperty("useOfVerbs");
        triggerCollider = serializedObject.FindProperty("triggerCollider");

        headerStyle = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold };
        headerStyle.normal.textColor = Color.white;
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.PropertyField(triggerCollider);

        EditorGUILayout.PropertyField(obj);

        if(obj != null && obj.objectReferenceValue != null)
        {
            InteractableObjEditor editor = (InteractableObjEditor)CreateEditor(obj.objectReferenceValue);

            editor.serializedObject.Update();

            editor.ObjectGUI();

            editor.serializedObject.ApplyModifiedProperties();
        }

        UseOfVerbsGUI();

        serializedObject.ApplyModifiedProperties();
    }

    protected void UseOfVerbsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.LabelField("Verbs can use on", headerStyle);

        GUILayout.FlexibleSpace();

        if (GUILayout.Button("+"))
        {
            verbs.arraySize++;
        }

        if (GUILayout.Button("Update Methods"))
        {
            behavior._UpdateMethods();
        }

        EditorGUILayout.EndHorizontal();

        if (verbs.isArray)
        {
            int elementCount = verbs.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty verb = verbs.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField("Verb " + (i + 1));

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    verbs.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                UseOfVerbGUI(verb, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void UseOfVerbGUI(SerializedProperty property, int i)
    {
        SerializedProperty verb = property.FindPropertyRelative("verb");
        SerializedProperty verbMovement = property.FindPropertyRelative("verbMovement");
        SerializedProperty useType = property.FindPropertyRelative("useType");

        SerializedProperty distanceFromObject = property.FindPropertyRelative("distanceFromObject");
        SerializedProperty pointToMove = property.FindPropertyRelative("pointToMove");

        SerializedProperty conversation = property.FindPropertyRelative("conversation");
        SerializedProperty methodID = property.FindPropertyRelative("methodID");

        EditorGUILayout.PropertyField(verb);

        if (verb.objectReferenceValue != null)
        {
            EditorGUILayout.PropertyField(verbMovement);

            switch ((VerbMovement)verbMovement.enumValueIndex)
            {
                case VerbMovement.DontMove:
                    break;
                case VerbMovement.MoveAround:
                    EditorGUILayout.PropertyField(distanceFromObject);
                    break;
                case VerbMovement.MoveToExactPoint:
                    EditorGUILayout.PropertyField(pointToMove);
                    break;
            }

            EditorGUILayout.PropertyField(useType);

            switch ((VerbResult)useType.enumValueIndex)
            {
                case VerbResult.StartConversation:
                    EditorGUILayout.PropertyField(conversation);
                    break;
                case VerbResult.PickObject:
                    break;
                case VerbResult.ExecuteMethod:
                    methodID.intValue = EditorGUILayout.Popup("Method to execute:", methodID.intValue, behavior.MethodNames);
                    break;
            }
        }
    }

    void AddVerb(SerializedProperty property, int i)
    {

    }
}
