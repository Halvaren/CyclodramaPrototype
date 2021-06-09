using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject oliverPrefab;
    public ThanksForPlayingUI thanksForPlayingUI;

    [Header("Intro to game settings")]
    public float cameraBlendingTime = 5f;
    public float spawnSetTimePercentage = 0.6f;

    private CameraManager cameraManager;
    public CameraManager CameraManager
    {
        get
        {
            if (cameraManager == null) cameraManager = CameraManager.instance;
            return cameraManager;
        }
    }

    private DataManager dataManager;
    public DataManager DataManager
    {
        get
        {
            if (dataManager == null) dataManager = DataManager.Instance;
            return dataManager;
        }
    }

    private SetTransitionSystem setTransitionSystem;
    public SetTransitionSystem SetTransitionSystem
    {
        get
        {
            if (setTransitionSystem == null) setTransitionSystem = SetTransitionSystem.instance;
            return setTransitionSystem;
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

    private TheaterCurtainBehavior theaterCurtain;
    public TheaterCurtainBehavior TheaterCurtain
    {
        get
        {
            if (theaterCurtain == null) theaterCurtain = TheaterCurtainBehavior.instance;
            return theaterCurtain;
        }
    }

    private TheaterGearsBehavior theaterGears;
    public TheaterGearsBehavior TheaterGears
    {
        get
        {
            if (theaterGears == null) theaterGears = TheaterGearsBehavior.instance;
            return theaterGears;
        }
    }

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
    }

    public void StartNewGame()
    {
        StartCoroutine(StartNewGameCoroutine());
    }

    IEnumerator StartNewGameCoroutine()
    {
        GeneralUIController.UnshowEverything();
        GeneralUIController.inventoryUIController.ResetInventoryUI();

        bool newScene = DataManager.pcData.newScene;
        GameObject setPrefab = newScene ? DataManager.setPrefabDictionary[(int)CharacterLocation.Corridor2] : DataManager.setPrefabDictionary[(int)DataManager.pcData.location];

        Vector3 initialPosition;
        if(newScene)
        {
            initialPosition = setPrefab.GetComponent<InitialSetBehavior>().newScenePosition.position;
        }
        else
        {
            initialPosition = new Vector3(DataManager.pcData.position[0], DataManager.pcData.position[1], DataManager.pcData.position[2]);
        }

        yield return StartCoroutine(FromIntroToMainCamera(cameraBlendingTime * spawnSetTimePercentage));

        TheaterCurtain.OpenCurtain();

        yield return StartCoroutine(SpawnOliverAndSet(newScene, initialPosition, setPrefab));

        DataManager.countingPlayedTime = true;
        DataManager.autosavingCounterActive = true;

        GeneralUIController.actionVerbsUIController.InitializeActionVerbs();
        GeneralUIController.ShowGameplayUI();
        GeneralUIController.displayNothing = false;
    }

    public void LoadOtherGame()
    {
        StartCoroutine(LoadOtherGameCoroutine());
    }

    IEnumerator LoadOtherGameCoroutine()
    {
        DataManager.countingPlayedTime = false;
        DataManager.autosavingCounterActive = false;

        GeneralUIController.UnshowEverything();
        GeneralUIController.inventoryUIController.ResetInventoryUI();

        bool newScene = DataManager.pcData.newScene;
        GameObject setPrefab = newScene ? DataManager.setPrefabDictionary[(int)CharacterLocation.Corridor2] : DataManager.setPrefabDictionary[(int)DataManager.pcData.location];
        
        Vector3 initialPosition;
        if (newScene)
        {
            initialPosition = setPrefab.GetComponent<InitialSetBehavior>().newScenePosition.position;
        }
        else
        {
            initialPosition = new Vector3(DataManager.pcData.position[0], DataManager.pcData.position[1], DataManager.pcData.position[2]);
        }

        TheaterCurtain.CloseCurtain();

        PCController oliver = PCController.instance;
        SetBehavior set = PCController.instance.currentSet;
        yield return DespawnOliverAndSet(oliver, set);

        yield return new WaitForSeconds(1f);

        TheaterCurtain.OpenCurtain();

        yield return SpawnOliverAndSet(newScene, initialPosition, setPrefab);

        DataManager.countingPlayedTime = true;
        DataManager.autosavingCounterActive = true;

        GeneralUIController.actionVerbsUIController.InitializeActionVerbs();
        GeneralUIController.ShowGameplayUI();
        GeneralUIController.displayNothing = false;
    }

    public void BackToMainMenu()
    {
        StartCoroutine(BackToMainMenuCoroutine());
    }

    IEnumerator BackToMainMenuCoroutine()
    {
        GeneralUIController.UnshowEverything();
        GeneralUIController.inventoryUIController.ResetInventoryUI();

        DataManager.countingPlayedTime = false;
        DataManager.autosavingCounterActive = false;

        float currentTime = Time.time;

        TheaterCurtain.CloseCurtain();
        yield return StartCoroutine(FromMainToIntroCamera(cameraBlendingTime * (1 - spawnSetTimePercentage)));

        PCController oliver = PCController.instance;
        SetBehavior set = PCController.instance.currentSet;
        yield return DespawnOliverAndSet(oliver, set);

        yield return new WaitForSeconds(cameraBlendingTime - (Time.time - currentTime));

        GeneralUIController.ShowMainMenuUI();
        GeneralUIController.displayNothing = false;
    }

    public void EndGame(EmployeeDoorBehavior door)
    {
        StartCoroutine(EndGameCoroutine(door));
    }

    IEnumerator EndGameCoroutine(EmployeeDoorBehavior door)
    {
        GeneralUIController.UnshowEverything();
        GeneralUIController.inventoryUIController.ResetInventoryUI();

        DataManager.countingPlayedTime = false;
        DataManager.autosavingCounterActive = false;

        PCController oliver = PCController.instance;
        DataManager.OnSaveData -= oliver.SavePCData;
        oliver.EnableGameplayInput(false);
        oliver.EnableInventoryInput(false);
        oliver.EnablePauseInput(false);
        oliver.MovementController.ActivateAgent(false);

        oliver.MovementController.ExitScene(2f, Vector3.back, false);

        yield return new WaitForSeconds(1f);

        yield return door.CloseDoor();

        Destroy(oliver.gameObject);

        SetBehavior set = door.currentSet.GetComponent<SetBehavior>();

        float currentTime = Time.time;

        TheaterCurtain.CloseCurtain();
        thanksForPlayingUI.ActivateUI(DataManager.loadedSaveStateData.playedTime);

        yield return StartCoroutine(FromMainToIntroCamera(cameraBlendingTime * (1 - spawnSetTimePercentage)));

        yield return StartCoroutine(DespawnOliverAndSet(null, set));

        yield return new WaitForSeconds(cameraBlendingTime - (Time.time - currentTime) + 5f);

        Application.Quit();
    }

    IEnumerator FromIntroToMainCamera(float waitingTime)
    {
        CameraManager.FromIntroToMainCamera();

        yield return new WaitForSeconds(waitingTime);
    }

    IEnumerator FromMainToIntroCamera(float waitingTime)
    {
        CameraManager.FromMainToIntroCamera();

        yield return new WaitForSeconds(waitingTime);
    }

    IEnumerator SpawnOliverAndSet(bool newScene, Vector3 initialPosition, GameObject setPrefab)
    {
        PCController oliver = null;

        TheaterGears.TurnOnOffGears(true);
        SetBehavior initialSet = SetTransitionSystem.InstantiateInitialSet(setPrefab, 20, SetTransitionSystem.setDisplacementTime);
        if (!newScene)
        {
            initialPosition.y -= 20;
            oliver = Instantiate(oliverPrefab, initialPosition, Quaternion.identity, initialSet.transform).GetComponent<PCController>();
            oliver.InitializePC();
            oliver.newScene = newScene;

            oliver.EnableGameplayInput(false);
            oliver.EnableInventoryInput(false);
            oliver.EnablePauseInput(false);

            oliver.MovementController.ActivateAgent(false);
        }

        initialSet.OnInstantiate();

        yield return new WaitForSeconds(SetTransitionSystem.setDisplacementTime);
        TheaterGears.TurnOnOffGears(false);

        if (newScene) initialPosition = initialSet.transform.TransformPoint(initialPosition);
        initialSet.RecalculateMesh();

        if (newScene && initialSet is InitialSetBehavior realInitialSet)
        {
            realInitialSet.employeeDoor.doorTrigger.gameObject.SetActive(false);

            oliver = Instantiate(oliverPrefab).GetComponent<PCController>();
            oliver.InitializePC();
            oliver.newScene = newScene;
            oliver.location = CharacterLocation.Corridor2;
            oliver.currentSet = realInitialSet;

            oliver.EnableGameplayInput(false);
            oliver.EnableInventoryInput(false);
            oliver.EnablePauseInput(false);

            oliver.MovementController.ActivateAgent(false);
            oliver.transform.position = initialPosition;

            yield return realInitialSet.employeeDoor._OpenDoorBeginningNewScene();

            yield return oliver.MovementController.MoveAndRotateToDirection(realInitialSet.employeeDoor.interactionPoint.position, Vector3.zero, true);

            yield return realInitialSet.employeeDoor.CloseDoor();

            realInitialSet.employeeDoor.doorTrigger.gameObject.SetActive(true);
        }

        initialSet.TurnOnOffLights(true);

        oliver.newScene = false;

        oliver.EnableGameplayInput(true);
        oliver.EnableInventoryInput(true);
        oliver.EnablePauseInput(true);
    }

    IEnumerator DespawnOliverAndSet(PCController oliver, SetBehavior set)
    {
        if (oliver != null)
        {
            DataManager.OnSaveData -= oliver.SavePCData;
            oliver.EnableGameplayInput(false);
            oliver.EnableInventoryInput(false);
            oliver.EnablePauseInput(false);

            oliver.transform.parent = set.transform;
        }

        set.TurnOnOffLights(false);
        TheaterGears.TurnOnOffGears(true);
        SetTransitionSystem.DisappearFinalSet(set.transform, 20, SetTransitionSystem.setDisplacementTime);

        yield return new WaitForSeconds(SetTransitionSystem.setDisplacementTime + 0.5f);
        TheaterGears.TurnOnOffGears(false);

        Destroy(set.gameObject);
    }
}
