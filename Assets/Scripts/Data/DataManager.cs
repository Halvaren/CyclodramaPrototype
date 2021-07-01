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
using System.Threading;

/// <summary>
/// Script responsible of collecting and storing game data, save it in files and load it from files
/// </summary>
public class DataManager : MonoBehaviour
{
    #region Variables

    //Object that will be loaded from files or saved in files
    [HideInInspector]
    public GameData gameData;

    //They will store game data during a game
    public Dictionary<int, SetData> setDatas = new Dictionary<int, SetData>();
    [HideInInspector]
    public InventoryData inventoryData;
    public Dictionary<int, NPCData> npcDatas = new Dictionary<int, NPCData>();
    [HideInInspector]
    public PCData pcData;

    //Current save state data
    [HideInInspector]
    public SaveStateData loadedSaveStateData;

    //Save state data references
    [HideInInspector]
    public SaveStateData defaultStateData;
    [HideInInspector]
    public SaveStateData autoSaveStateData;
    [HideInInspector]
    public List<SaveStateData> saveStateDatas;

    //Paths
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

    //Event executed when game needs to save
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

    #endregion

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

    /// <summary>
    /// First method to be executed
    /// </summary>
    /// <returns></returns>
    IEnumerator Init()
    {
        generalUIController.ShowLoadingUI(LoadingState.Loading);

        countingPlayedTime = false;
        autosavingCounterActive = false;
        LoadFileNames();
        yield return StartCoroutine(audioManager.FillPool());
        audioManager.PlayMenuMusic();
        yield return StartCoroutine(LoadSaveStateData());
    }

    private void Update()
    {
        //Increases played time
        if(countingPlayedTime && loadedSaveStateData != null)
        {
            loadedSaveStateData.playedTime += Time.deltaTime;
        }

        //Increases autosaving counter
        if(autosavingCounterActive)
        {
            autosavingCounter += Time.deltaTime;
        }
    }

    /// <summary>
    /// Check if it has to autosave
    /// </summary>
    /// <returns></returns>
    public bool HasToAutosave()
    {
        return autosavingCounter > autosavingLimitTime;
    }

    /// <summary>
    /// Returns inventory data stored in memory
    /// </summary>
    /// <returns></returns>
    public InventoryData GetInvenetoryData()
    {
        return inventoryData;
    }

    /// <summary>
    /// Sets inventory data stored in memory
    /// </summary>
    /// <param name="inventoryData"></param>
    public void SetInventoryData(InventoryData inventoryData)
    {
        this.inventoryData = inventoryData;
    }

    /// <summary>
    /// Returns set data from an ID stored in memory
    /// </summary>
    /// <param name="setID"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Sets set data from an ID stored in memory
    /// </summary>
    /// <param name="setID"></param>
    /// <param name="setData"></param>
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

    #region Load/Read methods

    /// <summary>
    /// Loads game dialogues
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadDialogues()
    {
        Debug.Log("Loading dialogues");

        yield return StartCoroutine(VD.LoadDialoguesCoroutine());
    }

    /// <summary>
    /// Gets file names in save path for further loads and saves
    /// </summary>
    void LoadFileNames()
    {
        saveFileNames = new List<string>();

        //Creates save path directory if it doesn't exist
        if (!Directory.Exists(completePathToSave))
        {
            Directory.CreateDirectory(completePathToSave);
        }

        string[] fileNames = Directory.GetFiles(completePathToSave, "save*.xml");

        foreach (string fileName in fileNames)
        {
            saveFileNames.Add(Path.GetFileName(fileName));
        }
    }

    /// <summary>
    /// Loads save state datas
    /// </summary>
    /// <returns></returns>
    IEnumerator LoadSaveStateData()
    {
        yield return StartCoroutine(ReadSaveStateDatas());

        generalUIController.UnshowLoadingUI();

        DataUIController.InitializeDataUI(autoSaveStateData, saveStateDatas);
        generalUIController.ShowMainMenuUI();
    }

    /// <summary>
    /// Loads auto save game data
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadAutoSaveGameData()
    {
        loadedSaveStateData = autoSaveStateData;
        yield return StartCoroutine(LoadGameData(completePathToSave + "/" + autoSaveFileName));
    }

    /// <summary>
    /// Loads new game data (default game data)
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadNewGameData()
    {
        loadedSaveStateData = new SaveStateData(defaultStateData);
        yield return StartCoroutine(LoadGameData(defaultSaveCompletePath));
        yield return StartCoroutine(SaveAutoSaveGameData(true));
    }

    /// <summary>
    /// Loads game data from an index
    /// </summary>
    /// <param name="fileIndex"></param>
    /// <returns></returns>
    public IEnumerator LoadGameData(int fileIndex)
    {
        if (fileIndex < 0 || fileIndex >= saveFileNames.Count) yield break;

        loadedSaveStateData = saveStateDatas[fileIndex];
        yield return StartCoroutine(LoadGameData(completePathToSave + "/" + saveFileNames[fileIndex]));
        yield return StartCoroutine(SaveAutoSaveGameData(true));
    }

    /// <summary>
    /// Loads game data from a path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    IEnumerator LoadGameData(string path)
    {
        yield return StartCoroutine(ReadDataFromPath(path));
        GetInfoFromGameData();

        yield return StartCoroutine(LoadDialogues());
    }

    /// <summary>
    /// Stores in memory data from GameData object. It is done after loading
    /// </summary>
    public void GetInfoFromGameData()
    {
        inventoryData = new InventoryData(gameData.inventoryData);
        pcData = new PCData(gameData.pcData);

        setDatas = new Dictionary<int, SetData>();
        foreach (SetData setData in gameData.setDatas)
        {
            setDatas.Add(setData.id, new SetData(setData));
        }

        npcDatas = new Dictionary<int, NPCData>();
        foreach (NPCData npcData in gameData.npcDatas)
        {
            if (npcData is NotanData notanData)
                npcDatas.Add(notanData.id, new NotanData(notanData));
            else if (npcData is MikeData mikeData)
                npcDatas.Add(mikeData.id, new MikeData(mikeData));
            else
                npcDatas.Add(npcData.id, new NPCData(npcData));
        }
    }

    SaveStateData auxiliarSaveStateData;

    /// <summary>
    /// Loads all save state datas (autosave save state, new game or default save state and regular game save states)
    /// </summary>
    /// <returns></returns>
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

        yield return StartCoroutine(ReadSaveStateData(autosaveFilePath));
        if (auxiliarSaveStateData != null)
        {
            autoSaveStateData = new SaveStateData(auxiliarSaveStateData);
        }
        else autoSaveStateData = null;

        yield return StartCoroutine(ReadSaveStateData(defaultSaveCompletePath));
        if(auxiliarSaveStateData != null)
            defaultStateData = new SaveStateData(auxiliarSaveStateData);
    }

    /// <summary>
    /// Loads save state data from path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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

                    /*auxiliarSaveStateData = new SaveStateData();

                    auxiliarSaveStateData.playedTime = GetFloatAttribute(dataElement, "playedTime");
                    Enum.TryParse(dataElement.GetAttribute("location"), out auxiliarSaveStateData.oliverLocation);
                    auxiliarSaveStateData.scene = GetIntegerAttribute(dataElement, "scene");
                    auxiliarSaveStateData.act = GetIntegerAttribute(dataElement, "act");*/

                    byte[] byteData = null;

                    //Reads bynary data in string and converts it to byte array
                    yield return new WaitForThreadedTask(() => byteData = ConvertStringToByteArray(saveDataElement.InnerText));

                    //Deserializes byte array, converting it in an object
                    yield return new WaitForThreadedTask(() => auxiliarSaveStateData = (SaveStateData)DeserializedObjectFromBinary(byteData));
                }
            }
        }
        else
        {
            Debug.LogWarning("Not found file in " + path);
        }
    }


    /// <summary>
    /// Loads GameData from path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Loads GameData from a XmlElement
    /// </summary>
    /// <param name="dataElement"></param>
    /// <returns></returns>
    IEnumerator ReadGameData(XmlElement dataElement)
    {
        XmlElement gameDataElement = (XmlElement)dataElement.SelectSingleNode(typeof(GameData).Name);

        byte[] byteData = null;

        //Reads bynary data in string and converts it to byte array
        yield return new WaitForThreadedTask(() => byteData = ConvertStringToByteArray(gameDataElement.InnerText));

        //Deserializes byte array, converting it in an object
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

    #region Save/Write methods

    /// <summary>
    /// Saves auto save game data
    /// </summary>
    /// <param name="dontGetGameObjectData">If true, it doesn't collect the current data from sets, NPCs, PC or inventory</param>
    /// <returns></returns>
    public IEnumerator SaveAutoSaveGameData(bool dontGetGameObjectData = false)
    {
        Debug.Log("Autosaving");
        autosavingCounter = 0.0f;
        yield return StartCoroutine(SaveGameData(completePathToSave + "/" + autoSaveFileName, dontGetGameObjectData));

        DataUIController.UpdateSaveState(-1, loadedSaveStateData);
        autoSaveStateData = loadedSaveStateData;
    }

    /// <summary>
    /// Saves game data from an index
    /// </summary>
    /// <param name="fileIndex"></param>
    /// <returns></returns>
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

        //If it's a new file, it has to create a new save state
        if (newFile)
        {
            SaveStateData newSaveState = new SaveStateData(loadedSaveStateData);
            saveStateDatas.Add(newSaveState);
            loadedSaveStateData = newSaveState;
        }

        DataUIController.UpdateSaveState(fileIndex, loadedSaveStateData);
    }

    /// <summary>
    /// Saves game data from a path
    /// </summary>
    /// <param name="path"></param>
    /// <param name="dontGetGameObjectData">If true, it doesn't collect the current data from sets, NPCs, PC or inventory</param>
    /// <returns></returns>
    IEnumerator SaveGameData(string path, bool dontGetGameObjectData = false)
    {
        if (!dontGetGameObjectData && OnSaveData != null)
        {
            OnSaveData();
        }
        loadedSaveStateData.oliverLocation = pcData.location;

        FillGameData();

        yield return StartCoroutine(WriteDataToPath(path));
    }

    /// <summary>
    /// Fills GameData object with the data stored in memory. It is done before saving
    /// </summary>
    public void FillGameData()
    {
        gameData.inventoryData = new InventoryData(inventoryData);
        gameData.pcData = new PCData(pcData);

        gameData.setDatas = new List<SetData>();
        foreach (int id in setDatas.Keys)
        {
            gameData.setDatas.Add(new SetData(setDatas[id]));
        }

        gameData.npcDatas = new List<NPCData>();
        foreach (int id in npcDatas.Keys)
        {
            if (npcDatas[id] is NotanData notanData)
                gameData.npcDatas.Add(new NotanData(notanData));
            else if (npcDatas[id] is MikeData mikeData)
                gameData.npcDatas.Add(new MikeData(mikeData));
            else
                gameData.npcDatas.Add(new NPCData(npcDatas[id]));
        }
    }

    /// <summary>
    /// Saves GameData to a path
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Saves current save state data as a XmlElement
    /// </summary>
    /// <param name="saveDoc"></param>
    /// <param name="dataElement"></param>
    /// <returns></returns>
    IEnumerator WriteSaveStateData(XmlDocument saveDoc, XmlElement dataElement)
    {
        XmlElement saveDataElement = saveDoc.CreateElement(loadedSaveStateData.GetType().Name);
        dataElement.AppendChild(saveDataElement);

        byte[] byteData = null; 

        //Serializes current save state data to a byte array
        yield return new WaitForThreadedTask(() => byteData = SerializeObjectToBinary(loadedSaveStateData));

        //Writes the byte array as string in a XmlElement
        yield return new WaitForThreadedTask(() => saveDataElement.InnerText = ConvertByteArrayToString(byteData));

        /*dataElement.SetAttribute("playedTime", loadedSaveStateData.playedTime.ToString());
        dataElement.SetAttribute("location", loadedSaveStateData.oliverLocation.ToString());
        dataElement.SetAttribute("scene", loadedSaveStateData.scene.ToString());
        dataElement.SetAttribute("act", loadedSaveStateData.act.ToString());

        yield return null;*/
    }

    /// <summary>
    /// Saves GameData as a XmlElement
    /// </summary>
    /// <param name="saveDoc"></param>
    /// <param name="dataElement"></param>
    /// <returns></returns>
    IEnumerator WriteGameData(XmlDocument saveDoc, XmlElement dataElement)
    {
        XmlElement gameDataElement = saveDoc.CreateElement(gameData.GetType().Name);
        dataElement.AppendChild(gameDataElement);

        byte[] byteData = null;

        //Serializes GameData to a byte array
        yield return new WaitForThreadedTask(() => byteData = SerializeObjectToBinary(gameData));

        //Writes the byte array as string in a XmlElement
        yield return new WaitForThreadedTask(() => gameDataElement.InnerText = ConvertByteArrayToString(byteData));
    }

    
    /*IEnumerator WriteSetData(XmlDocument saveDoc, XmlElement gameDataElement)
    {
        XmlElement setsElement = saveDoc.CreateElement("Sets");
        gameDataElement.AppendChild(setsElement);

        foreach (SetData setData in gameData.setDatas)
        {
            XmlElement setElement = saveDoc.CreateElement("Set");
            setElement.SetAttribute("id", setData.id.ToString());

            setsElement.AppendChild(setElement);

            //SetData setData = gameData.setDatas[setID];
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

        foreach(NPCData npcData in gameData.npcDatas)
        {
            //NPCData npcData = gameData.npcDatas[objID];
            //npcData.id = objID;

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
    }
    */

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
                foreach(string group in groups)
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

                            byte @byte = 0;
                            bool dontAdd = false;

                            try
                            {
                                @byte = Convert.ToByte(byteString, 16);
                            }
                            catch(Exception)
                            {
                                dontAdd = true;
                            }
                            if(!dontAdd) bytes.Add(@byte);
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

/// <summary>
/// Serializable dictionary (visible and editable in Inspector) of pairs of int and InteractableObj for pickableObjs
/// </summary>
[Serializable]
public class ObjDictionary : SerializableDictionaryBase<int, InteractableObj>
{
    
}

/// <summary>
/// Serializable dictionary (visible and editable in Inspector) of pairs of string and ActionVerb for verbs
/// </summary>
[Serializable]
public class VerbDictionary : SerializableDictionaryBase<string, ActionVerb>
{

}

/// <summary>
/// Serializable dictionary (visible and editable in Inspector) of pairs of int and GameObject for set prefabs
/// </summary>
[Serializable]
public class SetPrefabDictionary : SerializableDictionaryBase<int, GameObject>
{

}
