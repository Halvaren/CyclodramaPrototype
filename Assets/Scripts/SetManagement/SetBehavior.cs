using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetBehavior : MonoBehaviour
{
    protected Stack<bool> cutsceneLocks = new Stack<bool>();

    public int setID;

    public List<Light> setLighting;

    public List<InteractableObjBehavior> objBehaviors;
    public List<PickableObjBehavior> pickableObjBehaviors;
    public List<ContainerObjBehavior> containerObjBehaviors;
    public List<DoorBehavior> doorBehaviors;
    public List<NPCBehavior> npcBehaviors;
    public List<EmitterObjBehavior> emitterObjBehaviors;
    public List<DetailedObjBehavior> detailedObjBehaviors;

    SetData setData;

    public DataManager DataManager { get { return DataManager.Instance; } }

    public void InitializeSet()
    {
        setData = DataManager.GetSetData(setID);
        if(setData == null)
        {
            SaveSetData();
        }

        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.objDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        foreach(PickableObjBehavior behavior in pickableObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.pickableObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.pickableObjDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (ContainerObjBehavior behavior in containerObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.containerObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.containerObjDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (DoorBehavior behavior in doorBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.doorDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.npcDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.npcDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.emitterObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.emitterObjDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        foreach(DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.detailedObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.detailedObjDatas[behavior.obj.objID]);
                }
            }

            behavior.InitializeObjBehavior(gameObject);
        }

        DataManager.OnSaveData += SaveSetData;
    }

    public IEnumerator SetOnPlace()
    {
        PCController.instance.EnableGameplayInput(false);
        PCController.instance.EnableInventoryInput(false);

        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        foreach (PickableObjBehavior behavior in pickableObjBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        foreach (ContainerObjBehavior behavior in containerObjBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        foreach (DoorBehavior behavior in doorBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        foreach (NPCBehavior behavior in npcBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        foreach (DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            StartCoroutine(behavior.PlayInitialBehavior());
        }

        while(cutsceneLocks.Count > 0)
        {
            yield return null;
        }

        PCController.instance.EnableGameplayInput(true);
        PCController.instance.EnableInventoryInput(true);
    }

    public void OnBeforeSetChanging()
    {
        SaveSetData();
        TurnOnOffLights(false);
    }

    public void OnInstantiate()
    {
        InitializeSet(); 
        TurnOnOffLights(false);
    }

    public void OnAfterSetChanging()
    {
        SetOnPlace();
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

    public void TurnOnOffLights(float multiplier)
    {
        foreach(Light light in setLighting)
        {
            light.intensity *= multiplier;
        }
    }

    public void ModifyDoorData(int doorID, DoorData doorData)
    {
        setData = DataManager.GetSetData(setID);
        if (setData == null)
        {
            setData = new SetData();
        }

        foreach (DoorBehavior behavior in doorBehaviors)
        {
            if(behavior is SetDoorBehavior setDoor && setDoor.obj != null && setDoor.obj.objID ==  doorID)
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

    #region Save data methods

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

    void SaveInteractableObjData()
    {
        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if (behavior.obj != null)
            {
                InteractableObjData objData = behavior.GetObjData();
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
                PickableObjData pickableObjData = (PickableObjData)behavior.GetObjData();
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
                ContainerObjData containerObjData = (ContainerObjData)behavior.GetObjData();
                if (setData.containerObjDatas.ContainsKey(behavior.obj.objID))
                    setData.containerObjDatas[behavior.obj.objID] = containerObjData;

                else
                    setData.containerObjDatas.Add(behavior.obj.objID, containerObjData);
            }
        }
    }

    void SaveDoorObjData()
    {
        foreach (DoorBehavior behavior in doorBehaviors)
        {
            if (behavior.obj != null)
            {
                DoorData doorData = (DoorData)behavior.GetObjData();
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                    setData.doorDatas[behavior.obj.objID] = doorData;
                else
                    setData.doorDatas.Add(behavior.obj.objID, doorData);

                if(behavior is SetDoorBehavior setDoorBehavior)
                    setDoorBehavior.nextSet.GetComponent<SetBehavior>().ModifyDoorData(setDoorBehavior.obj.objID, doorData);
            }
        }
    }

    void SaveNPCData()
    {
        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                NPCData npcData = (NPCData)behavior.GetObjData();
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
                EmitterObjData emitterObjData = (EmitterObjData)behavior.GetObjData();
                if (setData.emitterObjDatas.ContainsKey(behavior.obj.objID))
                    setData.emitterObjDatas[behavior.obj.objID] = emitterObjData;
                else
                    setData.emitterObjDatas.Add(behavior.obj.objID, emitterObjData);
            }
        }
    }

    #endregion

    public void AddCutsceneLock()
    {
        cutsceneLocks.Push(true);
    }

    public void ReleaseCutsceneLock()
    {
        cutsceneLocks.Pop();
    }
}
