using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(InteractableObjBehavior)), CanEditMultipleObjects]
public class InteractableObjBehaviorEditor : Editor
{
    protected InteractableObjBehavior behavior;

    protected SerializedProperty inScene;
    protected SerializedProperty obj;
    protected SerializedProperty verbs;
    protected SerializedProperty triggerCollider;
    protected SerializedProperty obstacleCollider;
    protected SerializedProperty interactionPoint;
    protected SerializedProperty lookAtPoint;
    protected SerializedProperty currentSet;

    protected SerializedProperty objWeight;
    protected SerializedProperty objHeight;
    protected SerializedProperty characterVisibleToPick;

    protected SerializedProperty copyVerbsFrom;

    [SerializeField]
    protected GUIStyle headerStyle;
    public GUIStyle HeaderStyle
    {
        get
        {
            if (headerStyle == null) InitializeStyles();
            return headerStyle;
        }
    }

    [SerializeField]
    protected GUIStyle foldoutHeaderStyle;
    public GUIStyle FoldoutHeaderStyle
    {
        get
        {
            if (foldoutHeaderStyle == null) InitializeStyles();
            return foldoutHeaderStyle;
        }
    }

    protected static bool useOfVerbFoldout = true;

    protected void OnEnable()
    {
        InitializeEditor();
    }

    protected virtual void InitializeEditor()
    {
        behavior = (InteractableObjBehavior)target;

        inScene = serializedObject.FindProperty("inScene");
        obj = serializedObject.FindProperty("obj");
        verbs = serializedObject.FindProperty("useOfVerbs");
        triggerCollider = serializedObject.FindProperty("triggerCollider");
        interactionPoint = serializedObject.FindProperty("interactionPoint");
        lookAtPoint = serializedObject.FindProperty("lookAtPoint");
        obstacleCollider = serializedObject.FindProperty("obstacleCollider");
        currentSet = serializedObject.FindProperty("currentSet");

        objWeight = serializedObject.FindProperty("objWeight");
        objHeight = serializedObject.FindProperty("objHeight");
        characterVisibleToPick = serializedObject.FindProperty("characterVisibleToPick");

        copyVerbsFrom = serializedObject.FindProperty("copyVerbsFromBehavior");
    }

    protected virtual void InitializeStyles()
    {
        if(headerStyle == null)
        {
            headerStyle = new GUIStyle() { fontSize = 13, fontStyle = FontStyle.Bold };
            headerStyle.normal.textColor = Color.white;
        }

        if (foldoutHeaderStyle == null)
        {
            foldoutHeaderStyle = new GUIStyle(EditorStyles.foldoutHeader);
            foldoutHeaderStyle.fontSize = 13;
            foldoutHeaderStyle.fontStyle = FontStyle.Bold;
        }
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        if (EditorApplication.isPlaying)
        {
            GUI.enabled = false;
        }
        else
        {
            GUI.enabled = true;
        }

        EditorGUILayout.PropertyField(inScene);

        GUI.enabled = true;

        EditorGUILayout.PropertyField(interactionPoint);
        EditorGUILayout.PropertyField(lookAtPoint);

        EditorGUILayout.PropertyField(triggerCollider);
        EditorGUILayout.PropertyField(obstacleCollider);

        EditorGUILayout.PropertyField(obj);

        if(obj != null && obj.objectReferenceValue != null)
        {
            InteractableObjEditor editor = (InteractableObjEditor)CreateEditor(obj.objectReferenceValue);

            editor.serializedObject.Update();

            editor.ObjectGUI();

            editor.serializedObject.ApplyModifiedProperties();
        }

        EditorGUILayout.Space(15);

        UseOfVerbsGUI();

        serializedObject.ApplyModifiedProperties();
    }

    protected void UseOfVerbsGUI()
    {
        EditorGUILayout.BeginHorizontal();

        useOfVerbFoldout = EditorGUILayout.BeginFoldoutHeaderGroup(useOfVerbFoldout, "Verbs can use on", FoldoutHeaderStyle);

        EditorGUILayout.EndFoldoutHeaderGroup();

        if (useOfVerbFoldout)
        {
            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                verbs.arraySize++;
            }

            if (GUILayout.Button("Update Methods"))
            {
                behavior.UpdateMethods();
            }
            
            if(GUILayout.Button("Add all verbs"))
            {
                AddAllVerbs();
            }

            if(GUILayout.Button("Remove all verbs"))
            {
                verbs.arraySize = 0;
            }
        }        

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(15);

        EditorGUILayout.BeginHorizontal();

        EditorGUILayout.PropertyField(copyVerbsFrom);

        if(GUILayout.Button("Copy") && copyVerbsFrom.objectReferenceValue != null)
        {
            InteractableObjBehavior otherBehavior = (InteractableObjBehavior)copyVerbsFrom.objectReferenceValue;

            verbs.arraySize = 0;

            foreach(UseOfVerb verb in otherBehavior.useOfVerbs)
            {
                verbs.arraySize++;
                CloneUseOfVerb(verbs.GetArrayElementAtIndex(verbs.arraySize - 1), verb.CopyUseOfVerb());
            }

            copyVerbsFrom.objectReferenceValue = null;
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space(15);

        if (useOfVerbFoldout && verbs.isArray)
        {
            int elementCount = verbs.arraySize;

            for (int i = 0; i < elementCount; i++)
            {
                SerializedProperty verb = verbs.GetArrayElementAtIndex(i);

                EditorGUILayout.BeginHorizontal();

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

        if(useOfVerbFoldout && verbs.arraySize > 0)
        {
            EditorGUILayout.BeginHorizontal();

            GUILayout.FlexibleSpace();

            if (GUILayout.Button("+"))
            {
                verbs.arraySize++;
            }

            if (GUILayout.Button("Update Methods"))
            {
                behavior.UpdateMethods();
            }

            EditorGUILayout.EndHorizontal();
        }
    }

    void UseOfVerbGUI(SerializedProperty property, int i)
    {
        SerializedProperty verb = property.FindPropertyRelative("verb");
        SerializedProperty multiObj = property.FindPropertyRelative("multiObj");

        SerializedProperty verbMovement = property.FindPropertyRelative("verbMovement");
        SerializedProperty useType = property.FindPropertyRelative("useType");

        SerializedProperty distanceFromObject = property.FindPropertyRelative("distanceFromObject");
        SerializedProperty overrideInteractionPoint = property.FindPropertyRelative("overrideInteractionPoint");

        SerializedProperty conversation = property.FindPropertyRelative("conversation");
        SerializedProperty methodID = property.FindPropertyRelative("methodID");

        EditorGUILayout.PropertyField(verb);
        EditorGUILayout.PropertyField(multiObj);

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
                    EditorGUILayout.PropertyField(overrideInteractionPoint);
                    break;
            }

            EditorGUILayout.PropertyField(useType);

            switch ((VerbResult)useType.enumValueIndex)
            {
                case VerbResult.StartConversation:
                    EditorGUILayout.PropertyField(conversation);
                    break;
                case VerbResult.Think:
                    EditorGUILayout.PropertyField(conversation);
                    break;
                case VerbResult.PickObject:
                    break;
                case VerbResult.StealObject:
                    break;
                case VerbResult.ExecuteMethod:
                    methodID.intValue = EditorGUILayout.Popup("Method to execute:", methodID.intValue, behavior.MethodNames);
                    break;
            }
        }
    }

    void AddAllVerbs()
    {
        ICollection<ActionVerb> allVerbs = DataManager.Instance.verbsDictionary.Values;

        foreach(ActionVerb verb in allVerbs)
        {
            verbs.arraySize++;
            verbs.GetArrayElementAtIndex(verbs.arraySize - 1).FindPropertyRelative("verb").objectReferenceValue = verb;
        }
    }

    void CloneUseOfVerb(SerializedProperty property, UseOfVerb useOfVerb)
    {
        SerializedProperty verb = property.FindPropertyRelative("verb");
        SerializedProperty multiObj = property.FindPropertyRelative("multiObj");

        SerializedProperty verbMovement = property.FindPropertyRelative("verbMovement");
        SerializedProperty useType = property.FindPropertyRelative("useType");

        SerializedProperty distanceFromObject = property.FindPropertyRelative("distanceFromObject");
        SerializedProperty overrideInteractionPoint = property.FindPropertyRelative("overrideInteractionPoint");

        SerializedProperty conversation = property.FindPropertyRelative("conversation");
        SerializedProperty methodID = property.FindPropertyRelative("methodID");

        verb.objectReferenceValue = useOfVerb.verb;
        multiObj.boolValue = useOfVerb.multiObj;

        verbMovement.intValue = (int)useOfVerb.verbMovement;
        useType.intValue = (int)useOfVerb.useType;

        distanceFromObject.floatValue = useOfVerb.distanceFromObject;
        overrideInteractionPoint.objectReferenceValue = useOfVerb.overrideInteractionPoint;

        conversation.objectReferenceValue = useOfVerb.conversation;
        methodID.intValue = useOfVerb.methodID;
    }
}


[CustomEditor(typeof(SeatableObjBehavior)), CanEditMultipleObjects]
public class SeatableObjBehaviorEditor : InteractableObjBehaviorEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(WaterDispenserObjBehavior)), CanEditMultipleObjects]
public class WaterDispenserObjBehaviorEditor : InteractableObjBehaviorEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(BathroomElementObjBehavior)), CanEditMultipleObjects]
public class BathroomElementObjBehaviorEditor : InteractableObjBehaviorEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();
    }
}

[CustomEditor(typeof(CoffeeMachineObjBehavior)), CanEditMultipleObjects]
public class CoffeeMachineObjBehaviorEditor : InteractableObjBehaviorEditor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        EditorGUILayout.Space(15);

        base.OnInspectorGUI();
    }
}
