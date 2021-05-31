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

    private NavMeshSurface navMesh;
    public NavMeshSurface NavMesh
    {
        get
        {
            if (navMesh == null) navMesh = GetComponent<NavMeshSurface>();
            return navMesh;
        }
    }

    SetData setData;

    public DataManager DataManager { get { return DataManager.Instance; } }

    protected virtual void InitializeSet()
    {
        setData = DataManager.GetSetData(setID);
        if(setData == null)
        {
            SaveSetData();
        }

        LoadData();

        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        foreach(PickableObjBehavior behavior in pickableObjBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (ContainerObjBehavior behavior in containerObjBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (DoorBehavior behavior in doorBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (NPCBehavior behavior in npcBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        foreach(DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            behavior.InitializeObjBehavior(gameObject);
        }

        DataManager.OnSaveData += SaveData;
    }

    protected IEnumerator SetOnPlace()
    {
        PCController.instance.EnableGameplayInput(false);
        PCController.instance.EnableInventoryInput(false);

        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
        }

        foreach (PickableObjBehavior behavior in pickableObjBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
        }

        foreach (ContainerObjBehavior behavior in containerObjBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
        }

        foreach (DoorBehavior behavior in doorBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
        }

        foreach (NPCBehavior behavior in npcBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
        }

        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
        }

        foreach (DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            AddCutsceneLock();
            StartCoroutine(behavior._PlayInitialBehavior());
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
        SaveData();
        TurnOnOffLights(false);
    }

    public void OnInstantiate()
    {
        InitializeSet(); 
        TurnOnOffLights(false);
    }

    public void OnAfterSetChanging()
    {
        StartCoroutine(SetOnPlace());
        RecalculateMesh();
        TurnOnOffLights(true);
    }

    public void RecalculateMesh()
    {
        NavMesh.BuildNavMesh();
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

    public void SaveData()
    {
        SaveSetData();
        SaveNPCData();
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
        SaveEmitterObjData();
        SaveDetailedObjData();

        DataManager.SetSetData(setID, setData);
    }

    public void SaveNPCData()
    {
        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                NPCData npcData = (NPCData)behavior.GetObjData();
                if (DataManager.npcDatas.ContainsKey(behavior.obj.objID))
                {
                    DataManager.npcDatas[behavior.obj.objID] = npcData;
                }
                else
                {
                    DataManager.npcDatas.Add(behavior.obj.objID, npcData);
                }
            }
        }
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

    void SaveDetailedObjData()
    {
        foreach (DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            if (behavior.obj != null)
            {
                DetailedObjData detailedObjData = (DetailedObjData)behavior.GetObjData();
                if (setData.detailedObjDatas.ContainsKey(behavior.obj.objID))
                    setData.detailedObjDatas[behavior.obj.objID] = detailedObjData;
                else
                    setData.detailedObjDatas.Add(behavior.obj.objID, detailedObjData);
            }
        }
    }

    #endregion

    #region Load data methods

    public void LoadData()
    {
        LoadSetData();
        LoadNPCData();
    }

    public void LoadSetData()
    {
        LoadInteractableObjData();
        LoadPickableObjData();
        LoadDoorData();
        LoadContainerObjData();
        LoadEmitterObjData();
        LoadDetailedObjData();
    }

    public void LoadNPCData()
    {
        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                if (DataManager.npcDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(DataManager.npcDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadInteractableObjData()
    {
        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.objDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadPickableObjData()
    {
        foreach (PickableObjBehavior behavior in pickableObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.pickableObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.pickableObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadContainerObjData()
    {
        foreach (ContainerObjBehavior behavior in containerObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.containerObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.containerObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadEmitterObjData()
    {
        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.emitterObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.emitterObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadDetailedObjData()
    {
        foreach (DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.detailedObjDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.detailedObjDatas[behavior.obj.objID]);
                }
            }
        }
    }

    public void LoadDoorData()
    {
        foreach (DoorBehavior behavior in doorBehaviors)
        {
            if (behavior.obj != null)
            {
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                {
                    behavior.LoadData(setData.doorDatas[behavior.obj.objID]);
                }
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
