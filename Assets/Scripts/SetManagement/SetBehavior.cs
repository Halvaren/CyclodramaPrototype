using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetBehavior : MonoBehaviour
{
    public int setID;

    public List<Light> setLighting;

    public List<InteractableObjBehavior> objBehaviors;
    public List<DoorBehavior> doorBehaviors;
    public List<NPCController> npcBehaviors;

    public SetData setData;

    public SceneManager SceneManager { get { return SceneManager.instance; } }

    private void Start()
    {
        InitializeSet();
    }

    void InitializeSet()
    {
        setData = SceneManager.GetSetData(setID);
        if(setData == null)
        {
            SaveSetData();   
        }
        else
        {
            LoadSetData();
        }
    }

    public void OnBeforeSetChanging()
    {
        SaveSetData();
        TurnOnOffLights(false);
    }

    public void OnAfterSetChanging()
    {
        GetComponent<NavMeshSurface>().BuildNavMesh();
        TurnOnOffLights(true);
    }

    public void TurnOnOffLights(bool value)
    {
        foreach(Light light in setLighting)
        {
            light.enabled = value;
        }
    }

    public void ModifyDoorData(int doorID, DoorData doorData)
    {
        if(setData == null)
        {
            setData = new SetData();
        }

        foreach(DoorBehavior behavior in doorBehaviors)
        {
            if(behavior.obj != null && behavior.obj.objID ==  doorID)
            {
                if(setData.doorDatas.ContainsKey(doorID))
                {
                    setData.doorDatas[doorID] = doorData;
                }
                else
                {
                    setData.doorDatas.Add(doorID, doorData);
                }

                break;
            }
        }

        SceneManager.SetSetData(setID, setData);
    }

    public void SaveSetData()
    {
        if(setData == null)
        {
            setData = new SetData();
        }

        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if(behavior.obj != null)
            {
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                    setData.objDatas[behavior.obj.objID] = behavior._GetInteractableObjData();

                else
                    setData.objDatas.Add(behavior.obj.objID, behavior._GetInteractableObjData());
            }
        }

        foreach(DoorBehavior behavior in doorBehaviors)
        {
            if(behavior.obj != null)
            {
                DoorData doorData = behavior._GetDoorData();
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                    setData.doorDatas[behavior.obj.objID] = doorData;
                else
                    setData.doorDatas.Add(behavior.obj.objID, doorData);

                behavior.nextSet.GetComponent<SetBehavior>().ModifyDoorData(behavior.obj.objID, doorData);
            }
        }

        SceneManager.SetSetData(setID, setData);
    }

    public void LoadSetData()
    {
        bool aux = setID == 6;

        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if(behavior.obj != null)
            {
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.objDatas[behavior.obj.objID]);
                }
            }
        }

        foreach(DoorBehavior behavior in doorBehaviors)
        {
            if(behavior.obj != null)
            {
                if(setData.doorDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.doorDatas[behavior.obj.objID]);
                }
            }
        }
    }
}
