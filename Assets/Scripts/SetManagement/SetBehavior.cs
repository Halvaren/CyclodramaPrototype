using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SetBehavior : MonoBehaviour
{
    #region Variables

    //If there's any value in the stack, at the end of a set transition, it will wait until is empty to enable PC gameplay and inventory inputs
    protected Stack<bool> cutsceneLocks = new Stack<bool>();

    public int setID;

    public List<Light> setLighting;
    public AudioClip lightTurningOnClip;

    [Space(15)]

    public List<InteractableObjBehavior> objBehaviors;
    public List<PickableObjBehavior> pickableObjBehaviors;
    public List<ContainerObjBehavior> containerObjBehaviors;
    public List<DoorBehavior> doorBehaviors;
    public List<NPCBehavior> npcBehaviors;
    public List<EmitterObjBehavior> emitterObjBehaviors;
    public List<DetailedObjBehavior> detailedObjBehaviors;

    [Space(15)]
    [Tooltip("Order matters")]
    public List<BasicCutscene> cutscenes;

    private NavMeshSurface navMesh;
    public NavMeshSurface NavMesh
    {
        get
        {
            if (navMesh == null) navMesh = GetComponent<NavMeshSurface>();
            return navMesh;
        }
    }

    private GeneralUIController generalUIController;
    public GeneralUIController GeneralUIController
    {
        get
        {
            if (generalUIController == null) generalUIController = GeneralUIController.instance;
            return generalUIController;
        }
    }

    private AudioManager audioManager;
    public AudioManager AudioManager
    {
        get
        {
            if (audioManager == null) audioManager = AudioManager.instance;
            return audioManager;
        }
    }

    SetData setData;

    public DataManager DataManager { get { return DataManager.Instance; } }

    #endregion

    #region Methods

    /// <summary>
    /// Initializes the set. It is executed when the set is spawned (not confuse it with the set transition is done)
    /// </summary>
    protected virtual void InitializeSet()
    {
        //Gets SetData
        setData = DataManager.GetSetData(setID);
        if(setData == null)
        {
            SaveSetData();
        }

        //Load the needed data (SetData and NPCData)
        LoadData();

        //Transfer all data to its ObjBehaviors
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

    /// <summary>
    /// It is executed when the set transition that places this set on stage is done. It will execute cutscenes
    /// </summary>
    /// <returns></returns>
    public IEnumerator SetOnPlace()
    {
        PCController.instance.EnablePauseInput(false);
        PCController.instance.SetTransitionDone(setID);

        GeneralUIController.ShowLoadingUI(LoadingState.Autosaving);

        //Checks if game has to be autosaved
        if(DataManager.Instance.HasToAutosave())
            yield return StartCoroutine(DataManager.Instance.SaveAutoSaveGameData());

        GeneralUIController.UnshowLoadingUI();

        PCController.instance.EnablePauseInput(true);

        //Checks if there's any cutscene to run
        BasicCutscene cutsceneToRun = null;
        foreach(BasicCutscene cutscene in cutscenes)
        {
            if(cutscene.CheckStartConditions())
            {
                cutsceneToRun = cutscene;
                break;
            }
        }

        //If there's a cutscene
        if(cutsceneToRun != null)
        {
            //Runs it
            yield return StartCoroutine(cutsceneToRun.RunCutscene());
        }

        //Checks if there's any initial behavior of the ObjBehaviors to run
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
        
        //Waits for initial behaviors to be finished
        while(cutsceneLocks.Count > 0)
        {
            yield return null;
        }

        //Gives control to the player
        PCController.instance.EnableGameplayInput(true);
        PCController.instance.EnableInventoryInput(true);
    }

    /// <summary>
    /// Executed the moment a set transition begins, being this set the current one on stage
    /// </summary>
    public void OnBeforeSetChanging()
    {
        SaveData();
        TurnOnOffLights(false);
    }

    /// <summary>
    /// Executed when a set is instantiated
    /// </summary>
    public void OnInstantiate()
    {
        InitializeSet(); 
        TurnOnOffLights(false);

        if(PCController.instance != null)
            PCController.instance.currentSet = this;
    }

    /// <summary>
    /// Executed when a set transition is finished and this set is new current set on stage
    /// </summary>
    public void OnAfterSetChanging()
    {
        RecalculateMesh();
        TurnOnOffLights(true);
        StartCoroutine(SetOnPlace());
    }

    /// <summary>
    /// Recalculates the NavMeshSurface of the set
    /// </summary>
    public void RecalculateMesh()
    {
        NavMesh.BuildNavMesh();
    }

    /// <summary>
    /// Turns on or off the general lights of the set
    /// </summary>
    /// <param name="value"></param>
    public void TurnOnOffLights(bool value)
    {
        if (value) AudioManager.PlaySound(lightTurningOnClip, SoundType.MetaTheater);
        foreach(Light light in setLighting)
        {
            light.enabled = value;
        }
    }

    /// <summary>
    /// Multiplies general lights intensity by a multiplier
    /// </summary>
    /// <param name="multiplier"></param>
    public void TurnOnOffLights(float multiplier)
    {
        foreach(Light light in setLighting)
        {
            light.intensity *= multiplier;
        }
    }

    /// <summary>
    /// Loads some data of a specific SetDoor and transfer it to the connected SetDoor of another set
    /// </summary>
    /// <param name="doorID"></param>
    /// <param name="doorData"></param>
    public void ModifyDoorData(int doorID, DoorData doorData)
    {
        setData = DataManager.GetSetData(setID);
        if (setData == null)
        {
            setData = new SetData(setID);
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

    /// <summary>
    /// Saves all data (SetData and NPCData)
    /// </summary>
    public void SaveData()
    {
        SaveSetData();
        SaveNPCData();
    }

    /// <summary>
    /// Saves SetData
    /// </summary>
    public void SaveSetData()
    {
        setData = DataManager.GetSetData(setID);
        if (setData == null)
        {
            setData = new SetData(setID);
        }

        SaveInteractableObjData();
        SavePickableObjData();
        SaveDoorObjData();
        SaveContainerObjData();
        SaveEmitterObjData();
        SaveDetailedObjData();

        DataManager.SetSetData(setID, setData);
    }

    /// <summary>
    /// Saves NPCData
    /// </summary>
    public void SaveNPCData()
    {
        foreach (NPCBehavior behavior in npcBehaviors)
        {
            if (behavior.obj != null)
            {
                NPCData npcData = (NPCData)behavior.GetObjData();
                npcData.id = behavior.obj.objID;
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

    /// <summary>
    /// Saves InteractableObjs data
    /// </summary>
    void SaveInteractableObjData()
    {
        foreach (InteractableObjBehavior behavior in objBehaviors)
        {
            if (behavior.obj != null)
            {
                InteractableObjData objData = behavior.GetObjData();
                objData.id = behavior.obj.objID;
                if (setData.objDatas.ContainsKey(behavior.obj.objID))
                    setData.objDatas[behavior.obj.objID] = objData;

                else
                    setData.objDatas.Add(behavior.obj.objID, objData);
            }
        }
    }

    /// <summary>
    /// Saves PickableObjs data
    /// </summary>
    void SavePickableObjData()
    {
        foreach (InteractableObjBehavior behavior in pickableObjBehaviors)
        {
            if (behavior.obj != null)
            {
                PickableObjData pickableObjData = (PickableObjData)behavior.GetObjData();
                pickableObjData.id = behavior.obj.objID;
                if (setData.pickableObjDatas.ContainsKey(behavior.obj.objID))
                    setData.pickableObjDatas[behavior.obj.objID] = pickableObjData;

                else
                    setData.pickableObjDatas.Add(behavior.obj.objID, pickableObjData);
            }
        }
    }

    /// <summary>
    /// Saves ContainerObjs data
    /// </summary>
    void SaveContainerObjData()
    {
        foreach (InteractableObjBehavior behavior in containerObjBehaviors)
        {
            if (behavior.obj != null)
            {
                ContainerObjData containerObjData = (ContainerObjData)behavior.GetObjData();
                containerObjData.id = behavior.obj.objID;
                if (setData.containerObjDatas.ContainsKey(behavior.obj.objID))
                    setData.containerObjDatas[behavior.obj.objID] = containerObjData;

                else
                    setData.containerObjDatas.Add(behavior.obj.objID, containerObjData);
            }
        }
    }

    /// <summary>
    /// Saves Doors data
    /// </summary>
    void SaveDoorObjData()
    {
        foreach (DoorBehavior behavior in doorBehaviors)
        {
            if (behavior.obj != null)
            {
                DoorData doorData = (DoorData)behavior.GetObjData();
                doorData.id = behavior.obj.objID;
                if (setData.doorDatas.ContainsKey(behavior.obj.objID))
                    setData.doorDatas[behavior.obj.objID] = doorData;
                else
                    setData.doorDatas.Add(behavior.obj.objID, doorData);

                //In case it is a SetDoor, it has to transfer this data to its connected SetDoor in another set
                if(behavior is SetDoorBehavior setDoorBehavior && setDoorBehavior.nextSet != null)
                    setDoorBehavior.nextSet.GetComponent<SetBehavior>().ModifyDoorData(setDoorBehavior.obj.objID, doorData);
            }
        }
    }

    /// <summary>
    /// Saves EmitterObjs data
    /// </summary>
    void SaveEmitterObjData()
    {
        foreach (EmitterObjBehavior behavior in emitterObjBehaviors)
        {
            if (behavior.obj != null)
            {
                EmitterObjData emitterObjData = (EmitterObjData)behavior.GetObjData();
                emitterObjData.id = behavior.obj.objID;
                if (setData.emitterObjDatas.ContainsKey(behavior.obj.objID))
                    setData.emitterObjDatas[behavior.obj.objID] = emitterObjData;
                else
                    setData.emitterObjDatas.Add(behavior.obj.objID, emitterObjData);
            }
        }
    }

    /// <summary>
    /// Saves DetailedObjs data
    /// </summary>
    void SaveDetailedObjData()
    {
        foreach (DetailedObjBehavior behavior in detailedObjBehaviors)
        {
            if (behavior.obj != null)
            {
                DetailedObjData detailedObjData = (DetailedObjData)behavior.GetObjData();
                detailedObjData.id = behavior.obj.objID;
                if (setData.detailedObjDatas.ContainsKey(behavior.obj.objID))
                    setData.detailedObjDatas[behavior.obj.objID] = detailedObjData;
                else
                    setData.detailedObjDatas.Add(behavior.obj.objID, detailedObjData);
            }
        }
    }

    #endregion

    #region Load data methods

    /// <summary>
    /// Loads all data (SetData and NPCData)
    /// </summary>
    public void LoadData()
    {
        LoadSetData();
        LoadNPCData();
    }

    /// <summary>
    /// Loads SetData
    /// </summary>
    public void LoadSetData()
    {
        LoadInteractableObjData();
        LoadPickableObjData();
        LoadDoorData();
        LoadContainerObjData();
        LoadEmitterObjData();
        LoadDetailedObjData();
    }

    /// <summary>
    /// Loads NPCData
    /// </summary>
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

    /// <summary>
    /// Loads InteractableObjs data
    /// </summary>
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

    /// <summary>
    /// Loads PickableObjs data
    /// </summary>
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

    /// <summary>
    /// Loads ContainerObjs data
    /// </summary>
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

    /// <summary>
    /// Loads EmitterObjs data
    /// </summary>
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

    /// <summary>
    /// Loads DetailedObjs data
    /// </summary>
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

    /// <summary>
    /// Loads Doors data
    /// </summary>
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

    /// <summary>
    /// Adds a lock in the cutsceneLocks stack
    /// </summary>
    public void AddCutsceneLock()
    {
        cutsceneLocks.Push(true);
    }

    /// <summary>
    /// Removes a lock from the cutsceneLocks stack
    /// </summary>
    public void ReleaseCutsceneLock()
    {
        cutsceneLocks.Pop();
    }

    #endregion
}
