using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(NotanBehavior)), CanEditMultipleObjects]
public class NotanBehaviorEditor : NPCBehaviorEditor
{
    #region General variables

    protected SerializedProperty goneToBeMeasured;

    #endregion

    #region Dressing room variables

    protected SerializedProperty firstTimeTalk;
    protected SerializedProperty convinced;
    protected SerializedProperty incidentOccurred;

    protected SerializedProperty firstTimeConv;
    protected SerializedProperty secondTimeConv;
    protected SerializedProperty afterConvinceConv;
    protected SerializedProperty afterIncidentConv;

    protected SerializedProperty convinceConv;
    protected SerializedProperty giveDrinkConv;
    protected SerializedProperty throwDrinkConv;

    protected SerializedProperty chestRenderer;

    protected SerializedProperty notCutCupNodeIndex;
    protected SerializedProperty cutCupWithCoffeeNodeIndex;
    protected SerializedProperty cutCupWithWaterNodeIndex;

    protected SerializedProperty standUpPoint;
    protected SerializedProperty doorToCorridor1;
    protected SerializedProperty doorToBathroom;
    protected SerializedProperty stainedClothes;
    protected SerializedProperty kpopRecord;

    #endregion

    #region Costume workshop variables

    protected SerializedProperty moveAsidePlayerPosition;
    protected SerializedProperty doorToCorridor2;

    #endregion


    protected override void InitializeEditor()
    {
        base.InitializeEditor();

        goneToBeMeasured = serializedObject.FindProperty("goneToBeMeasured");

        firstTimeTalk = serializedObject.FindProperty("firstTimeTalk");
        convinced = serializedObject.FindProperty("convinced");
        incidentOccurred = serializedObject.FindProperty("incidentOccurred");

        firstTimeConv = serializedObject.FindProperty("firstTimeConv");
        secondTimeConv = serializedObject.FindProperty("secondTimeConv");
        afterConvinceConv = serializedObject.FindProperty("afterConvinceConv");
        afterIncidentConv = serializedObject.FindProperty("afterIncidentConv");

        convinceConv = serializedObject.FindProperty("convinceConv");
        giveDrinkConv = serializedObject.FindProperty("giveDrinkConv");
        throwDrinkConv = serializedObject.FindProperty("throwDrinkConv");

        chestRenderer = serializedObject.FindProperty("chestRenderer");

        notCutCupNodeIndex = serializedObject.FindProperty("notCutCupNodeIndex");
        cutCupWithCoffeeNodeIndex = serializedObject.FindProperty("cutCupWithCoffeeNodeIndex");
        cutCupWithWaterNodeIndex = serializedObject.FindProperty("cutCupWithWaterNodeIndex");

        standUpPoint = serializedObject.FindProperty("standUpPoint");
        doorToCorridor1 = serializedObject.FindProperty("doorToCorridor1");
        doorToBathroom = serializedObject.FindProperty("doorToBathroom");
        stainedClothes = serializedObject.FindProperty("stainedClothes");
        kpopRecord = serializedObject.FindProperty("kpopRecord");

        moveAsidePlayerPosition = serializedObject.FindProperty("moveAsidePlayerPosition");
        doorToCorridor2 = serializedObject.FindProperty("doorToCorridor2");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(15);

        serializedObject.Update();

        EditorGUILayout.LabelField("General variables", headerStyle);

        EditorGUILayout.PropertyField(goneToBeMeasured);

        EditorGUILayout.Space(15);

        EditorGUILayout.LabelField("Dressing room 1 variables");

        if(location.intValue == (int)NPCLocation.DressingRoom1)
        {
            EditorGUILayout.LabelField("State", headerStyle);

            EditorGUILayout.PropertyField(firstTimeTalk);
            EditorGUILayout.PropertyField(convinced);
            EditorGUILayout.PropertyField(incidentOccurred);

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField("Conversations", headerStyle);

            EditorGUILayout.PropertyField(firstTimeConv);
            EditorGUILayout.PropertyField(secondTimeConv);
            EditorGUILayout.PropertyField(afterConvinceConv);
            EditorGUILayout.PropertyField(afterIncidentConv);
            EditorGUILayout.PropertyField(convinceConv);
            EditorGUILayout.PropertyField(giveDrinkConv);
            EditorGUILayout.PropertyField(throwDrinkConv);

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField("Node triggers indexes", headerStyle);

            EditorGUILayout.PropertyField(notCutCupNodeIndex);
            EditorGUILayout.PropertyField(cutCupWithCoffeeNodeIndex);
            EditorGUILayout.PropertyField(cutCupWithWaterNodeIndex);

            EditorGUILayout.Space(15);

            EditorGUILayout.LabelField("Other variables", headerStyle);

            EditorGUILayout.PropertyField(chestRenderer);
            EditorGUILayout.PropertyField(standUpPoint);
            EditorGUILayout.PropertyField(doorToCorridor1);
            EditorGUILayout.PropertyField(doorToBathroom);
            EditorGUILayout.PropertyField(stainedClothes);
            EditorGUILayout.PropertyField(kpopRecord);
        }
        else if(location.intValue == (int)NPCLocation.CostumeWorkshop)
        {
            EditorGUILayout.PropertyField(moveAsidePlayerPosition);
            EditorGUILayout.PropertyField(doorToCorridor2);
        }
        else
        {
            EditorGUILayout.LabelField("Notan is not thought to be in that location");
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
