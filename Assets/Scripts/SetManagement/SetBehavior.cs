using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetBehavior : MonoBehaviour
{
    public int setID;

    public List<Light> setLighting;

    public List<InteractableObjBehavior> objBehaviors;
    public List<PickableObjBehavior> pickableObjBehaviors;
    public List<ContainerObjBehavior> containerObjBehaviors;
    public List<SetDoorBehavior> doorBehaviors;
    public List<NPCBehavior> npcBehaviors;
    public List<EmitterObjBehavior> emitterObjBehaviors;

    SetData setData;

    public DataManager DataManager { get { return DataManager.Instance; } }

    private void Start()
    {
        InitializeSet();
    }

    public void InitializeSet()
    {
        foreach(InteractableObjBehavior objBehavior in objBehaviors)
        {
            objBehavior.currentSet = gameObject;
        }

        foreach(PickableObjBehavior objBehavior in pickableObjBehaviors)
        {
            objBehavior.currentSet = gameObject;
        }

        foreach (ContainerObjBehavior objBehavior in containerObjBehaviors)
        {
            objBehavior.currentSet = gameObject;
        }

        foreach (SetDoorBehavior objBehavior in doorBehaviors)
        {
            objBehavior.currentSet = gameObject;
        }

        foreach (NPCBehavior objBehavior in npcBehaviors)
        {
            objBehavior.currentSet = gameObject;
        }

        foreach (EmitterObjBehavior objBehavior in emitterObjBehaviors)
        {
            objBehavior.currentSet = gameObject;
        }

        setData = DataManager.GetSetData(setID);
        if(setData == null)
        {
            SaveSetData();   
        }
        else
        {
            LoadSetData();
        }

        DataManager.OnSaveData += SaveSetData;
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
        setData = DataManager.GetSetData(setID);
        if (setData == null)
        {
            setData = new SetData();
        }

        foreach (SetDoorBehavior behavior in doorBehaviors)
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

        DataManager.SetSetData(setID, setData);
    }

    public void SaveSetData()
    {
        setData = DataManager.GetSetData(setID);
        if (setData == null)
        {
            setData = new SetData();
        }

        SaveInteractableObjData();
        SavePickableObjData();
        SaveDoorObjData();
        SaveContainerObjData();
        SaveNPCData();
        SaveEmitterObjData();

        DataManager.SetSetData(setID, setData);
    }

    #region Save data methods

    void SaveInteractableObjData()
    {
        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if (behavior.obj != null)
            {
                InteractableObjData objData = behavior._GetObjData();
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                    setData.objDatas[behavior.obj.objID] = objData;

                else
                    setData.objDatas.Add(behavior.obj.objID, objData);
            }
        }
    }

    void SavePickableObjData()
    {
        foreach (InteractableObjBehavior behavior in pickableObjBehaviors)
        {
            if (behavior.obj != null)
            {
                PickableObjData pickableObjData = (PickableObjData)behavior._GetObjData();
                if (setData.pickableObjDatas.ContainsKey(behavior.obj.objID))
                    setData.pickableObjDatas[behavior.obj.objID] = pickableObjData;

                else
                    setData.pickableObjDatas.Add(behavior.obj.objID, pickableObjData);
            }
        }
    }

    void SaveContainerObjData()
    {
        foreach (InteractableObjBehavior behavior in containerObjBehaviors)
        {
            if (behavior.obj != null)
            {
                ContainerObjData containerObjData = (ContainerObjData)behavior._GetObjData();
                if (setData.containerObjDatas.ContainsKey(behavior.obj.objID))
                    setData.containerObjDatas[behavior.obj.objID] = containerObjData;

                else
                    setData.containerObjDatas.Add(behavior.obj.objID, containerObjData);
            }
        }
    }

    void SaveDoorObjData()
    {
        foreach (SetDoorBehavior behavior in doorBehaviors)
        {
            if (behavior.obj != null)
            {
                DoorData doorData = (DoorData)behavior._GetObjData();
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                    setData.doorDatas[behavior.obj.objID] = doorData;
                else
                    setData.doorDatas.Add(behavior.obj.objID, doorData);

                behavior.nextSet.GetComponent<SetBehavior>().ModifyDoorData(behavior.obj.objID, doorData);
            }
        }
    }

    void SaveNPCData()
    {
        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                NPCData npcData = (NPCData)behavior._GetObjData();
                if (setData.npcDatas.ContainsKey(behavior.obj.objID))
                    setData.npcDatas[behavior.obj.objID] = npcData;
                else
                    setData.npcDatas.Add(behavior.obj.objID, npcData);
            }
        }
    }

    void SaveEmitterObjData()
    {
        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            if (behavior.obj != null)
            {
                EmitterObjData emitterObjData = (EmitterObjData)behavior._GetObjData();
                if (setData.emitterObjDatas.ContainsKey(behavior.obj.objID))
                    setData.emitterObjDatas[behavior.obj.objID] = emitterObjData;
                else
                    setData.emitterObjDatas.Add(behavior.obj.objID, emitterObjData);
            }
        }
    }

    #endregion

    public void LoadSetData()
    {

        LoadInteractableObjData();
        LoadContainerObjData();
        LoadDoorData();
        LoadEmitterObjData();
        LoadNPCData();
        LoadPickableObjData();
    }

    #region Load data methods

    void LoadInteractableObjData()
    {
        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.objDatas[behavior.obj.objID]);
                }
            }
        }
    }

    void LoadDoorData()
    {
        foreach (SetDoorBehavior behavior in doorBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.doorDatas[behavior.obj.objID]);
                }
            }
        }
    }

    void LoadNPCData()
    {
        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.npcDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.npcDatas[behavior.obj.objID]);
                }
            }
        }
    }

    void LoadEmitterObjData()
    {
        foreach(EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            if(behavior.obj != null)
            {
                if(setData.emitterObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.emitterObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadPickableObjData()
    {
        foreach(PickableObjBehavior behavior in pickableObjBehaviors)
        {
            if(behavior.obj != null)
            {
                if(setData.pickableObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.pickableObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    void LoadContainerObjData()
    {
        foreach(ContainerObjBehavior behavior in containerObjBehaviors)
        {
            if(behavior.obj != null)
            {
                if(setData.containerObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior._LoadData(setData.containerObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    #endregion
}
