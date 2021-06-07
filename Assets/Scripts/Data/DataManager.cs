using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine.SceneManagement;
using System;
using RotaryHeart.Lib.SerializableDictionary;
using VIDE_Data;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;

public class DataManager : MonoBehaviour
{
    [HideInInspector]
    public GameData gameData;

    public Dictionary<int, SetData> setDatas = new Dictionary<int, SetData>();
    [HideInInspector]
    public InventoryData inventoryData;
    public Dictionary<int, NPCData> npcDatas = new Dictionary<int, NPCData>();
    [HideInInspector]
    public PCData pcData;

    [HideInInspector]
    public SaveStateData loadedSaveStateData;

    public string pathToSave = "/saves";
    public string completePathToSave
    {
        get { return Application.persistentDataPath + pathToSave; }
    }

    public string defaultSaveCompletePath
    {
        get { return Application.streamingAssetsPath + "/" + defaultSaveFile; }
    }

    public string defaultSaveFile = "default.xml";
    public string autoSaveFileName = "autosave.xml";
    [HideInInspector]
    public List<string> saveFileNames;

    [HideInInspector]
    public SaveStateData defaultStateData;
    [HideInInspector]
    public SaveStateData autoSaveStateData;
    [HideInInspector]
    public List<SaveStateData> saveStateDatas;

    public float autosavingLimitTime = 300;
    float autosavingCounter;
    [HideInInspector]
    public bool autosavingCounterActive = false;

    [HideInInspector]
    public bool countingPlayedTime = false;

    public GeneralUIController generalUIController;
    public AudioManager audioManager;

    public VerbDictionary verbsDictionary;
    public ObjDictionary pickableObjsDictionary;
    public SetPrefabDictionary setPrefabDictionary;

    public delegate void SaveDataEvent();
    public static event SaveDataEvent OnSaveData;

    private DataUIController dataUIController;
    public DataUIController DataUIController
    {
        get
        {
            if (dataUIController == null) dataUIController = generalUIController.dataUIController;
            return dataUIController;
        }
    }

    private static DataManager instance;
    public static DataManager Instance
    {
        get
        {
            if (Application.isEditor && instance == null) return FindObjectOfType<DataManager>();
            return instance;
        }
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        if(instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        StartCoroutine(Init());
    }

    IEnumerator Init()
    {
        generalUIController.ShowLoadingUI(LoadingState.Loading);

        countingPlayedTime = false;
        autosavingCounterActive = false;
        LoadFileNames();
        yield return StartCoroutine(audioManager.FillPool());
        yield return StartCoroutine(LoadSaveStateData());
    }

    private void Update()
    {
        if(countingPlayedTime && loadedSaveStateData != null)
        {
            loadedSaveStateData.playedTime += Time.deltaTime;
        }

        if(autosavingCounterActive)
        {
            autosavingCounter += Time.deltaTime;
        }
    }

    public bool HasToAutosave()
    {
        return autosavingCounter > autosavingLimitTime;
    }

    void LoadFileNames()
    {
        saveFileNames = new List<string>();

        if(!Directory.Exists(completePathToSave))
        {
            Directory.CreateDirectory(completePathToSave);
        }

        string[] fileNames = Directory.GetFiles(completePathToSave, "save*.xml");

        foreach (string fileName in fileNames)
        {
            saveFileNames.Add(Path.GetFileName(fileName));
        }
    }

    public IEnumerator LoadAutoSaveGameData()
    {
        loadedSaveStateData = autoSaveStateData;
        yield return StartCoroutine(LoadGameData(completePathToSave + "/" + autoSaveFileName));
    }

    public IEnumerator LoadNewGameData()
    {
        loadedSaveStateData = new SaveStateData(defaultStateData);
        yield return StartCoroutine(LoadGameData(defaultSaveCompletePath));
        yield return StartCoroutine(SaveAutoSaveGameData());
    }

    public IEnumerator LoadGameData(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= saveFileNames.Count) yield break;

        loadedSaveStateData = saveStateDatas[fileIndex];
        yield return StartCoroutine(LoadGameData(completePathToSave + "/" + saveFileNames[fileIndex]));
    }

    IEnumerator LoadGameData(string path)
    {
        yield return StartCoroutine(ReadDataFromPath(path));
        GetInfoFromGameData();

        yield return StartCoroutine(LoadDialogues());
    }

    IEnumerator LoadSaveStateData()
    {
        yield return StartCoroutine(ReadSaveStateDatas());

        generalUIController.UnshowLoadingUI();

        DataUIController.InitializeDataUI(autoSaveStateData, saveStateDatas);
        generalUIController.ShowMainMenuUI();
    }

    public IEnumerator SaveAutoSaveGameData()
    {
        autosavingCounter = 0.0f;
        yield return StartCoroutine(SaveGameData(completePathToSave + "/" + autoSaveFileName));

        DataUIController.UpdateSaveState(-1, loadedSaveStateData);
        autoSaveStateData = loadedSaveStateData;
    }

    public IEnumerator SaveGameData(int fileIndex)
    {
        bool newFile = false;
        if (fileIndex < 0) yield break;
        if (fileIndex >= saveFileNames.Count)
        {
            saveFileNames.Add("save" + (saveFileNames.Count) + ".xml");
            newFile = true;
        }

        yield return StartCoroutine(SaveGameData(completePathToSave + "/" + saveFileNames[fileIndex]));

        if(newFile)
        {
            SaveStateData newSaveState = new SaveStateData(loadedSaveStateData);
            saveStateDatas.Add(newSaveState);
            loadedSaveStateData = newSaveState;
        }

        DataUIController.UpdateSaveState(fileIndex, loadedSaveStateData);
    }

    IEnumerator SaveGameData(string path)
    {
        if(OnSaveData != null)
            OnSaveData();
        loadedSaveStateData.oliverLocation = pcData.location;
        FillGameData();

        yield return StartCoroutine(WriteDataToPath(path));
    }

    public void FillGameData()
    {
        gameData.inventoryData = new InventoryData(inventoryData);
        gameData.pcData = new PCData(pcData);

        gameData.setDatas = new List<SetData>();
        foreach(int id in setDatas.Keys)
        {
            gameData.setDatas.Add(new SetData(setDatas[id]));
        }

        gameData.npcDatas = new List<NPCData>();
        foreach(int id in npcDatas.Keys)
        {
            gameData.npcDatas.Add(new NPCData(npcDatas[id]));
        }
    }

    public void GetInfoFromGameData()
    {
        inventoryData = new InventoryData(gameData.inventoryData);
        pcData = new PCData(gameData.pcData);

        setDatas = new Dictionary<int, SetData>();
        foreach(SetData setData in gameData.setDatas)
        {
            setDatas.Add(setData.id, new SetData(setData));
        }

        npcDatas = new Dictionary<int, NPCData>();
        foreach(NPCData npcData in gameData.npcDatas)
        {
            npcDatas.Add(npcData.id, new NPCData(npcData));
        }
    }

    public InventoryData GetInvenetoryData()
    {
        return inventoryData;
    }

    public void SetInventoryData(InventoryData inventoryData)
    {
        this.inventoryData = inventoryData;
    }

    public SetData GetSetData(int setID)
    {
        if (setDatas.ContainsKey(setID))
        {
            SetData setData = setDatas[setID];
            return setData;
        }
        else 
        {
            return null;
        }
    }

    public void SetSetData(int setID, SetData setData)
    {
        if(setDatas.ContainsKey(setID))
        {
            setDatas[setID] = setData;
        }
        else
        {
            setDatas.Add(setID, setData);
        }
    }

    IEnumerator LoadDialogues()
    {
        Debug.Log("Loading dialogues");
        yield return StartCoroutine(VD.LoadDialoguesCoroutine());
    }

    #region Read methods

    SaveStateData auxiliarSaveStateData;

    IEnumerator ReadSaveStateDatas()
    {
        Debug.Log("Reading save state datas");

        List<string> saveFilePaths = new List<string>();
        string autosaveFilePath = completePathToSave + "/" + autoSaveFileName;

        foreach(string fileName in saveFileNames)
        {
            saveFilePaths.Add(completePathToSave + "/" + fileName);
        }

        saveStateDatas = new List<SaveStateData>();

        foreach(string path in saveFilePaths)
        {
            yield return StartCoroutine(ReadSaveStateData(path));

            saveStateDatas.Add(new SaveStateData(auxiliarSaveStateData));
        }

        StartCoroutine(ReadSaveStateData(autosaveFilePath));
        autoSaveStateData = new SaveStateData(auxiliarSaveStateData);

        StartCoroutine(ReadSaveStateData(defaultSaveCompletePath));
        defaultStateData = new SaveStateData(auxiliarSaveStateData);
    }

    IEnumerator ReadSaveStateData(string path)
    {
        if (File.Exists(path))
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                XmlDocument data = new XmlDocument();

                try
                {
                    data.Load(fs);
                }
                catch (XmlException e)
                {
                    Debug.LogError(e);
                }

                if (data.FirstChild != null)
                {
                    XmlElement dataElement = (XmlElement)data.SelectSingleNode("//Data");

                    XmlElement saveDataElement = (XmlElement)dataElement.SelectSingleNode(typeof(SaveStateData).Name);

                    byte[] byteData = null;
                    yield return new WaitForThreadedTask(() => byteData = ConvertStringToByteArray(saveDataElement.InnerText));

                    yield return new WaitForThreadedTask(() => auxiliarSaveStateData = (SaveStateData)DeserializedObjectFromBinary(byteData));
                }
            }
        }
        else
        {
            Debug.LogWarning("Not found file in " + path);
        }
    }


    IEnumerator ReadDataFromPath(string path)
    {
        Debug.Log("Reading game data");
        if(File.Exists(path))
        {
            using (FileStream fs = new FileStream(path, FileMode.Open))
            {
                XmlDocument data = new XmlDocument();
                 
                try
                {
                    data.Load(fs);
                }
                catch (XmlException e)
                {
                    Debug.LogError(e);
                }

                if(data.FirstChild != null)
                {
                    XmlElement dataElement = (XmlElement)data.SelectSingleNode("//Data");

                    yield return StartCoroutine(ReadGameData(dataElement));
                    /*yield return StartCoroutine(ReadSetData(dataElement));
                    yield return StartCoroutine(ReadInventoryData(dataElement));
                    yield return StartCoroutine(ReadNPCsData(dataElement));
                    yield return StartCoroutine(ReadPCData(dataElement));*/
                }
            }
        }
        else
        {
            Debug.LogWarning("Not found file in " + path);
        }
    }

    IEnumerator ReadGameData(XmlElement dataElement)
    {
        XmlElement gameDataElement = (XmlElement)dataElement.SelectSingleNode(typeof(GameData).Name);

        byte[] byteData = null;
        yield return new WaitForThreadedTask(() => byteData = ConvertStringToByteArray(gameDataElement.InnerText));

        yield return new WaitForThreadedTask(() => gameData = (GameData)DeserializedObjectFromBinary(byteData));
    }

    /*IEnumerator ReadSetData(XmlElement gameDataElement)
    {
        setDatas.Clear();

        XmlElement setsElement = (XmlElement)gameDataElement.SelectSingleNode("Sets");

        IEnumerable<XmlElement> setElements = setsElement.SelectNodes("Set").OfType<XmlElement>();

        foreach(XmlElement setElement in setElements)
        {
            int id = GetIntegerAttribute(setElement, "id");
            SetData setData = new SetData(id);

            setDatas.Add(id, setData);

            XmlElement interactableObjsElement = (XmlElement)setElement.SelectSingleNode("InteractableObjs");

            if(interactableObjsElement != null)
            {
                IEnumerable<XmlElement> interactableObjElements = interactableObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement interactableObjElement in interactableObjElements)
                {
                    Type type = null;
                    if (interactableObjElement.Name == typeof(InteractableObjData).Name) type = typeof(InteractableObjData);
                    else if (interactableObjElement.Name == typeof(SeatableObjData).Name) type = typeof(SeatableObjData);

                    if(type != null)
                    {
                        InteractableObjData objData = (InteractableObjData)DeserializeObjectFromXML(interactableObjElement, type, false);
                        setData.objDatas.Add(objData.id, objData);
                    }

                    yield return null;
                }
            }

            XmlElement pickableObjsElement = (XmlElement)setElement.SelectSingleNode("PickableObjs");

            if(pickableObjsElement != null)
            {
                IEnumerable<XmlElement> pickableObjElements = pickableObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach (XmlElement pickableObjElement in pickableObjElements)
                {
                    Type type = null;
                    if (pickableObjElement.Name == typeof(PickableObjData).Name) type = typeof(PickableObjData);
                    else if (pickableObjElement.Name == typeof(TeddyBearObjData).Name) type = typeof(TeddyBearObjData);
                    else if (pickableObjElement.Name == typeof(RopeObjData).Name) type = typeof(RopeObjData);
                    else if (pickableObjElement.Name == typeof(FabricObjData).Name) type = typeof(FabricObjData);

                    if (type != null)
                    {
                        PickableObjData pickableObjData = (PickableObjData)DeserializeObjectFromXML(pickableObjElement, type, false);
                        setData.pickableObjDatas.Add(pickableObjData.id, pickableObjData);
                    }

                    yield return null;
                }
            }

            XmlElement doorsElement = (XmlElement)setElement.SelectSingleNode("Doors");

            if(doorsElement != null)
            {
                IEnumerable<XmlElement> doorElements = doorsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement doorElement in doorElements)
                {
                    Type type = null;
                    if (doorElement.Name == typeof(DoorData).Name) type = typeof(DoorData);

                    if (type != null)
                    {
                        DoorData doorData = (DoorData)DeserializeObjectFromXML(doorElement, type, false);
                        setData.doorDatas.Add(doorData.id, doorData);
                    }

                    yield return null;
                }
            }

            XmlElement containerObjsElement = (XmlElement)setElement.SelectSingleNode("ContainerObjs");

            if(containerObjsElement != null)
            {
                IEnumerable<XmlElement> containerObjElements = containerObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement containerObjElement in containerObjElements)
                {
                    Type type = null;
                    if (containerObjElement.Name == typeof(ContainerObjData).Name) type = typeof(ContainerObjData);

                    if (type != null)
                    {
                        ContainerObjData containerObjData = (ContainerObjData)DeserializeObjectFromXML(containerObjElement, type, false);
                        setData.containerObjDatas.Add(containerObjData.id, containerObjData);
                    }

                    yield return null;
                }
            }

            XmlElement emitterObjsElement = (XmlElement)setElement.SelectSingleNode("EmitterObjs");

            if(emitterObjsElement != null)
            {
                IEnumerable<XmlElement> emitterObjElements = emitterObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach(XmlElement emitterObjElement in emitterObjElements)
                {
                    Type type = null;
                    if (emitterObjElement.Name == typeof(EmitterObjData).Name) type = typeof(EmitterObjData);
                    else if (emitterObjElement.Name == typeof(OpenableEmmitterObjData).Name) type = typeof(OpenableEmmitterObjData);

                    if (type != null)
                    {
                        EmitterObjData emitterObjData = (EmitterObjData)DeserializeObjectFromXML(emitterObjElement, type, false);
                        setData.emitterObjDatas.Add(emitterObjData.id, emitterObjData);
                    }

                    yield return null;
                }
            }

            XmlElement detailedObjsElement = (XmlElement)setElement.SelectSingleNode("DetailedObjs");

            if (detailedObjsElement != null)
            {
                IEnumerable<XmlElement> detailedObjElements = detailedObjsElement.SelectNodes("*").OfType<XmlElement>();

                foreach (XmlElement detailedObjElement in detailedObjElements)
                {
                    Type type = null;
                    if (detailedObjElement.Name == typeof(DetailedObjData).Name) type = typeof(DetailedObjData);
                    else if (detailedObjElement.Name == typeof(DetailedEmitterObjData).Name) type = typeof(DetailedEmitterObjData);
                    else if (detailedObjElement.Name == typeof(ZodiacBoxObjData).Name) type = typeof(ZodiacBoxObjData);

                    if (type != null)
                    {
                        DetailedObjData detailedObjData = (DetailedObjData)DeserializeObjectFromXML(detailedObjElement, type, false);
                        setData.detailedObjDatas.Add(detailedObjData.id, detailedObjData);
                    }

                    yield return null;
                }
            }

            yield return null;
        }
    }

    IEnumerator ReadInventoryData(XmlElement gameDataElement)
    {
        inventoryData = new InventoryData();

        XmlElement inventoryElement = (XmlElement)gameDataElement.SelectSingleNode("Inventory");

        if(inventoryElement != null)
        {
            IEnumerable<XmlElement> pickableObjInInventoryElements = inventoryElement.SelectNodes("*").OfType<XmlElement>();

            foreach (XmlElement pickableObjInInventoryElement in pickableObjInInventoryElements)
            {
                Type type = null;
                if (pickableObjInInventoryElement.Name == typeof(PickableObjData).Name) type = typeof(PickableObjData);
                else if (pickableObjInInventoryElement.Name == typeof(TeddyBearObjData).Name) type = typeof(TeddyBearObjData);
                else if (pickableObjInInventoryElement.Name == typeof(RopeObjData).Name) type = typeof(RopeObjData);
                else if (pickableObjInInventoryElement.Name == typeof(FabricObjData).Name) type = typeof(FabricObjData);

                if (type != null)
                {
                    PickableObjData pickableObjData = (PickableObjData)DeserializeObjectFromXML(pickableObjInInventoryElement, type, false);
                    inventoryData.pickableObjInInventoryDatas.Add(pickableObjData.id, pickableObjData);
                }

                yield return null;
            }
        }
    }

    IEnumerator ReadNPCsData(XmlElement gameDataElement)
    {
        XmlElement npcsElement = (XmlElement)gameDataElement.SelectSingleNode("NPCs");

        if(npcsElement != null)
        {
            IEnumerable<XmlElement> npcElements = npcsElement.SelectNodes("*").OfType<XmlElement>();

            foreach (XmlElement npcElement in npcElements)
            {
                Type type = null;
                if (npcElement.Name == typeof(NPCData).Name) type = typeof(NPCData);
                else if (npcElement.Name == typeof(NotanData).Name) type = typeof(NotanData);

                if (type != null)
                {
                    NPCData npcData = (NPCData)DeserializeObjectFromXML(npcElement, type, false);
                    npcDatas.Add(npcData.id, npcData);
                }

                yield return null;
            }
        }
    }

    IEnumerator ReadPCData(XmlElement gameDataElement)
    {
        XmlElement pcElement = (XmlElement)gameDataElement.SelectSingleNode("PC");

        if(pcElement != null)
        {
            pcData = new PCData((PCData)DeserializeObjectFromXML(pcElement, typeof(PCData)));

            yield return null;
        }
    }*/

    #endregion

    #region Write methods

    IEnumerator WriteDataToPath(string path)
    {
        XmlDocument saveDoc = new XmlDocument();
        XmlDeclaration decl = saveDoc.CreateXmlDeclaration("1.0", "UFT-8", null);
        saveDoc.InsertBefore(decl, saveDoc.DocumentElement);

        XmlElement dataElement = saveDoc.CreateElement("Data");
        saveDoc.AppendChild(dataElement);

        yield return StartCoroutine(WriteSaveStateData(saveDoc, dataElement));
        yield return StartCoroutine(WriteGameData(saveDoc, dataElement));
        /*yield return StartCoroutine(WriteSetData(saveDoc, dataElement));
        yield return StartCoroutine(WriteInventoryData(saveDoc, dataElement));
        yield return StartCoroutine(WriteNPCsData(saveDoc, dataElement));
        yield return StartCoroutine(WritePCData(saveDoc, dataElement));*/

        Directory.CreateDirectory(Path.GetDirectoryName(path));
        using (XmlTextWriter writer = new XmlTextWriter(path, Encoding.UTF8))
        {
            writer.Formatting = Formatting.Indented;
            writer.Indentation = 1;
            writer.IndentChar = '\t';
            saveDoc.Save(writer);
        }
    }

    IEnumerator WriteSaveStateData(XmlDocument saveDoc, XmlElement dataElement)
    {
        XmlElement saveDataElement = saveDoc.CreateElement(loadedSaveStateData.GetType().Name);
        dataElement.AppendChild(saveDataElement);

        byte[] byteData = null; 
        yield return new WaitForThreadedTask(() => byteData = SerializeObjectToBinary(loadedSaveStateData));

        yield return new WaitForThreadedTask(() => saveDataElement.InnerText = ConvertByteArrayToString(byteData));

        /*dataElement.SetAttribute("playedTime", loadedSaveStateData.playedTime.ToString());
        dataElement.SetAttribute("location", loadedSaveStateData.oliverLocation.ToString());
        dataElement.SetAttribute("scene", loadedSaveStateData.scene.ToString());
        dataElement.SetAttribute("act", loadedSaveStateData.act.ToString());*/
    }

    IEnumerator WriteGameData(XmlDocument saveDoc, XmlElement dataElement)
    {
        XmlElement gameDataElement = saveDoc.CreateElement(gameData.GetType().Name);
        dataElement.AppendChild(gameDataElement);

        byte[] byteData = null;
        yield return new WaitForThreadedTask(() => byteData = SerializeObjectToBinary(gameData));

        yield return new WaitForThreadedTask(() => gameDataElement.InnerText = ConvertByteArrayToString(byteData));
    }

    /*IEnumerator WriteSetData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement setsElement = saveDoc.CreateElement("Sets");
        gameDataElement.AppendChild(setsElement);

        foreach (int setID in gameData.setDatas.Keys)
        {
            XmlElement setElement = saveDoc.CreateElement("Set");
            setElement.SetAttribute("id", setID.ToString());

            setsElement.AppendChild(setElement);

            SetData setData = gameData.setDatas[setID];
            if (setData.objDatas.Count > 0)
            {
                XmlElement interactableObjsElement = saveDoc.CreateElement("InteractableObjs");

                setElement.AppendChild(interactableObjsElement);

                foreach (int objID in setData.objDatas.Keys)
                {
                    InteractableObjData objData = setData.objDatas[objID];
                    objData.id = objID;

                    SerializeObjectToXML(interactableObjsElement, objData);

                    yield return null;
                }
            }

            if (setData.pickableObjDatas.Count > 0)
            {
                XmlElement pickableObjsElement = saveDoc.CreateElement("PickableObjs");

                setElement.AppendChild(pickableObjsElement);

                foreach (int objID in setData.pickableObjDatas.Keys)
                {
                    PickableObjData pickableObjData = setData.pickableObjDatas[objID];
                    pickableObjData.id = objID;

                    SerializeObjectToXML(pickableObjsElement, pickableObjData);

                    yield return null;
                }
            }

            if(setData.doorDatas.Count > 0)
            {
                XmlElement doorsElement = saveDoc.CreateElement("Doors");

                setElement.AppendChild(doorsElement);

                foreach(int objID in setData.doorDatas.Keys)
                {
                    DoorData doorData = setData.doorDatas[objID];
                    doorData.id = objID;

                    SerializeObjectToXML(doorsElement, doorData);

                    yield return null;
                }
            }

            if (setData.containerObjDatas.Count > 0)
            {
                XmlElement containerObjsElement = saveDoc.CreateElement("ContainerObjs");

                setElement.AppendChild(containerObjsElement);

                foreach (int objID in setData.containerObjDatas.Keys)
                {
                    ContainerObjData containerObjData = setData.containerObjDatas[objID];
                    containerObjData.id = objID;

                    SerializeObjectToXML(containerObjsElement, containerObjData);

                    yield return null;
                }
            }

            if (setData.emitterObjDatas.Count > 0)
            {
                XmlElement emitterObjsElement = saveDoc.CreateElement("EmitterObjs");

                setElement.AppendChild(emitterObjsElement);

                foreach (int objID in setData.emitterObjDatas.Keys)
                {
                    EmitterObjData emitterObjData = setData.emitterObjDatas[objID];
                    emitterObjData.id = objID;

                    SerializeObjectToXML(emitterObjsElement, emitterObjData);

                    yield return null;
                }
            }

            if(setData.detailedObjDatas.Count > 0)
            {
                XmlElement detailedObjsElement = saveDoc.CreateElement("DetailedObjs");

                setElement.AppendChild(detailedObjsElement);

                foreach(int objID in setData.detailedObjDatas.Keys)
                {
                    DetailedObjData detailedObjData = setData.detailedObjDatas[objID];
                    detailedObjData.id = objID;

                    SerializeObjectToXML(detailedObjsElement, detailedObjData);

                    yield return null;
                }
            }

            yield return null;
        }
    }

    IEnumerator WriteInventoryData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement inventoryElement = saveDoc.CreateElement("Inventory");
        gameDataElement.AppendChild(inventoryElement);

        foreach(int objID in gameData.inventoryData.pickableObjInInventoryDatas.Keys)
        {
            PickableObjData pickableObjData = gameData.inventoryData.pickableObjInInventoryDatas[objID];
            pickableObjData.id = objID;

            SerializeObjectToXML(inventoryElement, pickableObjData);

            yield return null;
        }
    }

    IEnumerator WriteNPCsData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement npcsElement = saveDoc.CreateElement("NPCs");
        gameDataElement.AppendChild(npcsElement);

        foreach(int objID in gameData.npcDatas.Keys)
        {
            NPCData npcData = gameData.npcDatas[objID];
            npcData.id = objID;

            SerializeObjectToXML(npcsElement, npcData);

            yield return null;
        }
    }

    IEnumerator WritePCData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement pcElement = saveDoc.CreateElement("PC");
        gameDataElement.AppendChild(pcElement);

        PCData pcData = new PCData(gameData.pcData);

        SerializeObjectToXML(pcElement, pcData);

        yield return null;
    }*/

    #endregion

    #region Xml attributes methods

    /*private XmlElement SerializeObjectToXML(XmlElement parent, object obj)
    {
        try
        {
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", ""); 
            XmlSerializer serializer = new XmlSerializer(obj.GetType());
            XPathNavigator navigator = parent.CreateNavigator();
            using (XmlWriter writer = navigator.AppendChild())
            {
                writer.WriteWhitespace("");
                serializer.Serialize(writer, obj, ns);
            }
            return (XmlElement)parent.LastChild;
        }
        catch(Exception)
        {
            Debug.Log("Could not serialize " + obj.ToString());
            return null;
        }
    }*/

    private byte[] SerializeObjectToBinary(object obj)
    {
        if (obj == null)
            return null;

        try
        {
            byte[] result = null;
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream())
            {
                bf.Serialize(ms, obj);
                result = ms.ToArray();
            }
            return result;
        }
        catch(Exception)
        {
            Debug.Log("Could not serialize " + obj.ToString());
            return null;
        }
    }

    private string ConvertByteArrayToString(byte[] byteData)
    {
        string byteDataString = "";
        int bytesPerGroup = 0;
        int groupsPerLine = 0;
        int bytes = 0;
        for (int i = 0; i < byteData.Length; i++)
        {
            byteDataString += byteData[i].ToString("X2");
            bytes++;
            bytesPerGroup++;

            if(bytesPerGroup == 2)
            {
                bytesPerGroup = 0;
                groupsPerLine++;

                if(groupsPerLine == 4)
                {
                    byteDataString += '\n';
                    groupsPerLine = 0;
                }
                else
                {
                    byteDataString += " ";
                }
            }
        }

        return byteDataString;
    }

    /*private object DeserializeObjectFromXML(XmlElement xmlElement, Type type, bool isParent = true)
    {
        if (isParent && !xmlElement.HasChildNodes)
            return null;
        XmlSerializer serializer = new XmlSerializer(type);
        XPathNavigator navigator = (isParent ? xmlElement.FirstChild : xmlElement).CreateNavigator();
        using (XmlReader reader = navigator.ReadSubtree())
            return serializer.Deserialize(reader);
    }*/

    private object DeserializedObjectFromBinary(byte[] array)
    {
        if (array == null) return null;

        object result = null;
        BinaryFormatter bf = new BinaryFormatter();
        using (MemoryStream ms = new MemoryStream(array))
        {
            result = bf.Deserialize(ms);
        }

        return result;
    }

    private byte[] ConvertStringToByteArray(string array)
    {
        List<byte> bytes = new List<byte>();

        string[] lines = array.Split('\n');
        foreach (string line in lines)
        {
            if (line.Length == 0) continue;
            else
            {
                string[] groups = line.Split(' ');
                foreach (string group in groups)
                {
                    if (group.Length == 0) continue;
                    else
                    {
                        for (int i = 0; i < group.Length; i += 2)
                        {
                            string byteString = group[i].ToString();

                            if ((i + 1) < group.Length)
                            {
                                byteString += group[i + 1].ToString();
                            }

                            bytes.Add(Convert.ToByte(byteString, 16));
                        }
                    }
                }
            }
        }

        return bytes.ToArray();
    }

    private float GetFloatAttribute(XmlElement element, string attribute, bool throwIfInvalid = true)
    {
        float result = 0;
        if (!float.TryParse(element.GetAttribute(attribute), out result) && throwIfInvalid)
            throw new XmlException("Invalid Float " + attribute + " for element " + element.Name + "!");
        return result;
    }

    private int GetIntegerAttribute(XmlElement element, string attribute, bool throwIfInvalid = true)
    {
        int result = 0;
        if (!int.TryParse(element.GetAttribute(attribute), out result) && throwIfInvalid)
            throw new XmlException("Invalid Int " + attribute + " for element " + element.Name + "!");
        return result;
    }

    private bool GetBooleanAttribute(XmlElement element, string attribute, bool throwIfInvalid = true)
    {
        bool result = false;
        if (!bool.TryParse(element.GetAttribute(attribute), out result) && throwIfInvalid)
            throw new XmlException("Invalid Bool " + attribute + " for element " + element.Name + "!");
        return result;
    }

    #endregion
}

[Serializable]
public class ObjDictionary : SerializableDictionaryBase<int, InteractableObj>
{
    
}

[Serializable]
public class VerbDictionary : SerializableDictionaryBase<string, ActionVerb>
{

}

[Serializable]
public class SetPrefabDictionary : SerializableDictionaryBase<int, GameObject>
{

}
