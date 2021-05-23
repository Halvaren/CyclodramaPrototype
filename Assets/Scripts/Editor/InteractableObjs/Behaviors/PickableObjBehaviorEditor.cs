using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(PickableObjBehavior), true), CanEditMultipleObjects]
public class PickableObjBehaviorEditor : InteractableObjBehaviorEditor
{
    protected SerializedProperty inventoryObj;

    protected SerializedProperty useObjRelations;
    protected SerializedProperty giveObjRelations;
    protected SerializedProperty hitObjRelations;
    protected SerializedProperty drawObjRelations;
    protected SerializedProperty throwObjRelations;

    protected SerializedProperty defaultUseComment;
    protected SerializedProperty defaultDrawComment;
    protected SerializedProperty defaultHitComment;
    protected SerializedProperty defaultThrowComment;
    protected SerializedProperty defaultGiveComment;
    protected SerializedProperty cannotPickComment;

    [SerializeField]
    protected static bool useObjRelationsFoldout = true;
    [SerializeField]
    protected static bool giveObjRelationsFoldout = true;
    [SerializeField]
    protected static bool hitObjRelationsFoldout = true;
    [SerializeField]
    protected static bool drawObjRelationsFoldout = true;
    [SerializeField]
    protected static bool throwObjRelationsFoldout = true;

    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        inventoryObj = serializedObject.FindProperty("inventoryObj");

        useObjRelations = serializedObject.FindProperty("useObjRelations");
        giveObjRelations = serializedObject.FindProperty("giveObjRelations");
        hitObjRelations = serializedObject.FindProperty("hitObjRelations");
        drawObjRelations = serializedObject.FindProperty("drawObjRelations");
        throwObjRelations = serializedObject.FindProperty("throwObjRelations");

        defaultUseComment = serializedObject.FindProperty("defaultUseComment");
        defaultDrawComment = serializedObject.FindProperty("defaultDrawComment");
        defaultHitComment = serializedObject.FindProperty("defaultHitComment");
        defaultThrowComment = serializedObject.FindProperty("defaultThrowComment");
        defaultGiveComment = serializedObject.FindProperty("defaultGiveComment");
        cannotPickComment = serializedObject.FindProperty("cannotPickComment");
    }

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();

        serializedObject.Update();

        EditorGUILayout.Space(15);

        if (EditorApplication.isPlaying)
        {
            GUI.enabled = false;
        }
        else
        {
            GUI.enabled = true;
        }

        EditorGUILayout.PropertyField(inventoryObj);
        EditorGUILayout.PropertyField(objWeight);
        EditorGUILayout.PropertyField(objHeight);
        EditorGUILayout.PropertyField(characterVisibleToPick);

        GUI.enabled = true;

        EditorGUILayout.Space(15);

        EditorGUILayout.PropertyField(defaultUseComment);
        EditorGUILayout.PropertyField(defaultGiveComment);
        EditorGUILayout.PropertyField(defaultHitComment);
        EditorGUILayout.PropertyField(defaultDrawComment);
        EditorGUILayout.PropertyField(defaultThrowComment);
        EditorGUILayout.PropertyField(cannotPickComment);

        EditorGUILayout.Space(15);

        UseObjRelationsGUI();

        EditorGUILayout.Space(15);

        GiveObjRelationsGUI();

        EditorGUILayout.Space(15);

        HitObjRelationsGUI();

        EditorGUILayout.Space(15);

        DrawObjRelationsGUI();

        EditorGUILayout.Space(15);

        ThrowObjRelationGUI();

        serializedObject.ApplyModifiedProperties();
    }

    void UseObjRelationsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        useObjRelationsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(useObjRelationsFoldout, "Use object relations", FoldoutHeaderStyle);

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (useObjRelationsFoldout)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                useObjRelations.arraySize++;
            }
        }       

        EditorGUILayout.EndHorizontal(); 
        
        if (useObjRelationsFoldout && useObjRelations.isArray)
        {
            int elementCount = useObjRelations.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty useObjRelation = useObjRelations.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    useObjRelations.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                ObjRelationGUI(useObjRelation, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void GiveObjRelationsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        giveObjRelationsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(giveObjRelationsFoldout, "Give object relations", FoldoutHeaderStyle);

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (giveObjRelationsFoldout)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                giveObjRelations.arraySize++;
            }
        }

        EditorGUILayout.EndHorizontal();

        if (giveObjRelationsFoldout && giveObjRelations.isArray)
        {
            int elementCount = giveObjRelations.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty giveObjRelation = giveObjRelations.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    giveObjRelations.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                ObjRelationGUI(giveObjRelation, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void HitObjRelationsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        hitObjRelationsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(hitObjRelationsFoldout, "Hit object relations", FoldoutHeaderStyle);

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (hitObjRelationsFoldout)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                hitObjRelations.arraySize++;
            }
        }        

        EditorGUILayout.EndHorizontal();

        if (hitObjRelationsFoldout && hitObjRelations.isArray)
        {
            int elementCount = hitObjRelations.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty hitObjRelation = hitObjRelations.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    hitObjRelations.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                ObjRelationGUI(hitObjRelation, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void DrawObjRelationsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        drawObjRelationsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(drawObjRelationsFoldout, "Draw object relations", FoldoutHeaderStyle);

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (drawObjRelationsFoldout)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                drawObjRelations.arraySize++;
            }
        }

        EditorGUILayout.EndHorizontal();

        if (drawObjRelationsFoldout && drawObjRelations.isArray)
        {
            int elementCount = drawObjRelations.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty drawObjRelation = drawObjRelations.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    drawObjRelations.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                ObjRelationGUI(drawObjRelation, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void ThrowObjRelationGUI()
    {
        EditorGUILayout.BeginHorizontal();

        throwObjRelationsFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(throwObjRelationsFoldout, "Throw object relations", FoldoutHeaderStyle);

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (throwObjRelationsFoldout)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                throwObjRelations.arraySize++;
            }
        }        

        EditorGUILayout.EndHorizontal();

        if (throwObjRelationsFoldout && throwObjRelations.isArray)
        {
            int elementCount = throwObjRelations.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty throwObjRelation = throwObjRelations.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

                GUILayout.FlexibleSpace();

                if (GUILayout.Button("x"))
                {
                    throwObjRelations.DeleteArrayElementAtIndex(i);
                    break;
                }

                EditorGUILayout.EndHorizontal();

                ObjRelationGUI(throwObjRelation, i);

                EditorGUILayout.Space(15);
            }
        }
    }

    void ObjRelationGUI(SerializedProperty property, int i)
    {
        SerializedProperty index = property.FindPropertyRelative("index");
        SerializedProperty objs = property.FindPropertyRelative("objs");

        SerializedProperty objSet = property.FindPropertyRelative("objSet");

        EditorGUILayout.PropertyField(index);
        EditorGUILayout.PropertyField(objSet);

        if((ObjRelationSet)objSet.enumValueIndex == ObjRelationSet.ListOfObjs)
            EditorGUILayout.PropertyField(objs);
    }
}
