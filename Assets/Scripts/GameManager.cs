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

    public Animator curtainAnimator;
    public Animator gearsAnimator;

    bool init = false;

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
            if (generalUIController == null) generalUIController = GeneralUIController.Instance;
            return generalUIController;
        }
    }

    public static GameManager instance;

    private void Awake()
    {
        instance = this;
        gameContainer.SetActive(false);
    }

    private void Update()
    {
        if(!init && Input.GetKeyDown(KeyCode.Alpha4))
        {
            init = true;
            StartCoroutine(FromIntroToGameCoroutine());
        }
    }

    IEnumerator FromIntroToGameCoroutine()
    {
        GeneralUIController.gameObject.SetActive(false);

        bool newScene = DataManager.pcData.newScene;
        GameObject setPrefab = newScene ? DataManager.setPrefabDictionary[(int)CharacterLocation.Corridor2] : DataManager.setPrefabDictionary[(int)DataManager.pcData.location];
        Vector3 initialPosition = newScene ? setPrefab.GetComponent<InitialSetBehavior>().newScenePosition.position : DataManager.pcData.position;

        CameraManager.FromIntroToMainCamera();
        curtainAnimator.SetTrigger("openCurtain");
        gearsAnimator.SetBool("turningGears", true);

        float remainingTime = cameraBlendingTime - (cameraBlendingTime * spawnSetTimePercentage);
        yield return new WaitForSeconds(cameraBlendingTime * spawnSetTimePercentage);

        gameContainer.SetActive(true);

        remainingTime -= SetTransitionSystem.setDisplacementTime;

        PCController oliver = null;

        SetBehavior initialSet = SetTransitionSystem.InstantiateInitialSet(setPrefab, SetTransitionMovement.Towards, 20, SetTransitionSystem.setDisplacementTime);
        if(!newScene)
        {
            initialPosition.y -= 20;
            oliver = Instantiate(oliverPrefab, initialPosition, Quaternion.identity, initialSet.transform).GetComponent<PCController>();
            oliver.InitializePC();
            oliver.newScene = newScene;

            oliver.EnableGameplayInput(false);
            oliver.EnableInventoryInput(false);

            oliver.MovementController.ActivateAgent(false);
        }

        initialSet.OnInstantiate();

        yield return new WaitForSeconds(SetTransitionSystem.setDisplacementTime);

        if(newScene) initialPosition = initialSet.transform.TransformPoint(initialPosition);
        initialSet.RecalculateMesh();

        if(newScene && initialSet is InitialSetBehavior realInitialSet)
        {
            oliver = Instantiate(oliverPrefab, gameContainer.transform).GetComponent<PCController>();
            oliver.InitializePC();
            oliver.newScene = newScene;
            oliver.location = CharacterLocation.Corridor2;

            oliver.EnableGameplayInput(false);
            oliver.EnableInventoryInput(false);

            oliver.MovementController.ActivateAgent(false);
            oliver.transform.position = initialPosition;
        
            yield return realInitialSet.employeeDoor._OpenDoorBeginningNewScene();

            yield return oliver.MovementController.MoveAndRotateToPoint(realInitialSet.employeeDoor.interactionPoint.position, Vector3.zero, true);

            yield return realInitialSet.employeeDoor.CloseDoor();
        }

        initialSet.TurnOnOffLights(true);

        oliver.newScene = false;
        oliver.transform.parent = gameContainer.transform;

        oliver.EnableGameplayInput(true);
        oliver.EnableInventoryInput(true);

        GeneralUIController.gameObject.SetActive(true);
    }
}
