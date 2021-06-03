using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public GameObject gameContainer;
    public GameObject oliverPrefab;

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
        gameContainer.SetActive(false);
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
        Vector3 initialPosition = newScene ? setPrefab.GetComponent<InitialSetBehavior>().newScenePosition.position : DataManager.pcData.position;

        yield return StartCoroutine(FromIntroToMainCamera(cameraBlendingTime * spawnSetTimePercentage));

        gameContainer.SetActive(true);

        TheaterCurtain.OpenCurtain();

        yield return StartCoroutine(SpawnOliverAndSet(newScene, initialPosition, setPrefab));

        DataManager.countingPlayedTime = true;

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

        GeneralUIController.UnshowEverything();
        GeneralUIController.inventoryUIController.ResetInventoryUI();

        bool newScene = DataManager.pcData.newScene;
        GameObject setPrefab = newScene ? DataManager.setPrefabDictionary[(int)CharacterLocation.Corridor2] : DataManager.setPrefabDictionary[(int)DataManager.pcData.location];
        Vector3 initialPosition = newScene ? setPrefab.GetComponent<InitialSetBehavior>().newScenePosition.position : DataManager.pcData.position;

        TheaterCurtain.CloseCurtain();

        PCController oliver = PCController.instance;
        SetBehavior set = gameContainer.GetComponentInChildren<SetBehavior>();
        yield return DespawnOliverAndSet(oliver, set);

        yield return new WaitForSeconds(1f);

        TheaterCurtain.OpenCurtain();

        yield return SpawnOliverAndSet(newScene, initialPosition, setPrefab);

        DataManager.countingPlayedTime = true;

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

        float currentTime = Time.time;

        TheaterCurtain.CloseCurtain();
        yield return StartCoroutine(FromMainToIntroCamera(cameraBlendingTime * (1 - spawnSetTimePercentage)));

        PCController oliver = PCController.instance;
        SetBehavior set = gameContainer.GetComponentInChildren<SetBehavior>();
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
        yield return StartCoroutine(FromMainToIntroCamera(cameraBlendingTime * (1 - spawnSetTimePercentage)));

        yield return StartCoroutine(DespawnOliverAndSet(null, set));

        yield return new WaitForSeconds(cameraBlendingTime - (Time.time - currentTime));

        //Thanks for playing!
    }

    IEnumerator FromIntroToMainCamera(float waitingTime)
    {
        CameraManager.FromIntroToMainCamera();
        TheaterGears.TurnOnOffGears(true);

        yield return new WaitForSeconds(waitingTime);

        TheaterGears.TurnOnOffGears(false);
    }

    IEnumerator FromMainToIntroCamera(float waitingTime)
    {
        CameraManager.FromMainToIntroCamera();
        TheaterGears.TurnOnOffGears(true);

        yield return new WaitForSeconds(waitingTime);

        TheaterGears.TurnOnOffGears(false);
    }

    IEnumerator SpawnOliverAndSet(bool newScene, Vector3 initialPosition, GameObject setPrefab)
    {
        PCController oliver = null;

        SetBehavior initialSet = SetTransitionSystem.InstantiateInitialSet(setPrefab, SetTransitionMovement.Towards, 20, SetTransitionSystem.setDisplacementTime);
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

        if (newScene) initialPosition = initialSet.transform.TransformPoint(initialPosition);
        initialSet.RecalculateMesh();

        if (newScene && initialSet is InitialSetBehavior realInitialSet)
        {
            realInitialSet.employeeDoor.doorTrigger.gameObject.SetActive(false);

            oliver = Instantiate(oliverPrefab, gameContainer.transform).GetComponent<PCController>();
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
        oliver.transform.parent = gameContainer.transform;

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
        SetTransitionSystem.DisappearFinalSet(set.transform, SetTransitionMovement.Backwards, 20, SetTransitionSystem.setDisplacementTime);

        yield return new WaitForSeconds(SetTransitionSystem.setDisplacementTime + 0.5f);

        Destroy(set.gameObject);
    }
}
