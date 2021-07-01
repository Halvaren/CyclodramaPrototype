using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// First cutscene played when a new game starts. This cutscene is divided in two different sets. That means there are two gameobjects, each one in a different set,
/// with this script as a component and they have different values in their fields
/// </summary>
public class InitialCutscene : BasicCutscene
{
    #region Variables

    [Header("Characters")]
    public YaizaBehavior Yaiza;
    public BelindaBehavior Belinda;
    public RaulBehavior Raul;
    public VeronicaBehavior Veronica;
    public LeonBehavior Leon;

    [Header("Conversations")]
    public VIDE_Assign oliverIntroduction;
    public VIDE_Assign yaizaToOliver;
    public VIDE_Assign staffConversation;

    [Header("Doors")]
    public SetDoorBehavior corridor1Door;
    public SetDoorBehavior costumeWorkshopDoor;
    public SetDoorBehavior employeeZoneDoor;
    public SetDoorBehavior corridor2Door;

    [Header("Interaction points")]
    public Transform yaizaStopPoint;
    public Transform oliverStopPoint;

    [Header("Audio")]
    public AudioClip oliverTheme;
    public AudioClip leonTheme;

    private PCController oliver;
    public PCController Oliver
    {
        get
        {
            if (oliver == null) oliver = PCController.instance;
            return oliver;
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

    #endregion

    /// <summary>
    /// Runs the cutscene
    /// </summary>
    /// <returns></returns>
    public override IEnumerator RunCutscene()
    {
        //If it's located in corridor 2
        if(location == SetLocation.Corridor2)
        {
            //Plays Óliver theme
            AudioSource oliverThemeSource = AudioManager.PlaySound(oliverTheme, SoundType.ForegroundMusic);

            yield return new WaitForSeconds(0.5f);

            //Óliver introduces himself
            yield return StartCoroutine(corridor1Door._StartConversation(oliverIntroduction));
            AudioManager.FadeOutSound(oliverThemeSource, 1f, 0.5f);

            //Yaiza enters
            yield return StartCoroutine(Yaiza.SpawnFromDoor(true, corridor1Door, Vector3.right, 10f, false));
            yield return StartCoroutine(Yaiza.GoToPoint(yaizaStopPoint.position, true));

            //Yaiza talks to Óliver
            yield return StartCoroutine(Yaiza.MovementController.RotateToDirectionCoroutine(Oliver.transform.position - Yaiza.transform.position));
            yield return StartCoroutine(Yaiza._StartConversation(yaizaToOliver));

            //Yaiza exits through employee zone door
            yield return StartCoroutine(Yaiza.GoToDoorAndExit(true, employeeZoneDoor, Vector3.forward, false, 1.5f));

            AudioManager.FadeOutSound(oliverThemeSource, 3f);
            //Blocks corridor 1 door and costume workshop door
            corridor1Door.transitionTrigger.cantGoThrough = true;
            costumeWorkshopDoor.transitionTrigger.cantGoThrough = true;

            //This part of the cutscene is finished
            Oliver.pcData.corridor2InitialCutsceneActive = false;
        }
        //If it's located in employee zone
        else if(location == SetLocation.EmployeeZone)
        {
            //Moves Óliver to his position
            yield return StartCoroutine(Oliver.MovementController.MoveAndRotateToPoint(oliverStopPoint.position, Leon.transform.position));

            //Plays Léon theme
            AudioSource leonThemeSource = AudioManager.PlaySound(leonTheme, SoundType.ForegroundMusic);
            //León starts conversation
            yield return StartCoroutine(Leon._StartConversation(staffConversation));
            AudioManager.FadeOutSound(leonThemeSource, 3f);

            //All character leave
            StartCoroutine(Raul.GoToDoorAndExit(false, corridor2Door, Vector3.forward, false, 1.5f));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Belinda.GoToDoorAndExit(false, corridor2Door, Vector3.forward, false, 1.5f));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Leon.GoToDoorAndExit(false, corridor2Door, Vector3.forward, false, 1.5f));
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(Yaiza.GoToDoorAndExit(false, corridor2Door, Vector3.forward, false, 1.5f));
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(Veronica.GoToDoorAndExit(false, corridor2Door, Vector3.forward, false, 1.5f));
            yield return new WaitForSeconds(0.5f);

            //This part of the cutscene is finished
            Oliver.pcData.employeeZoneInitialCutsceneActive = false;
        }

        yield return null;
    }

    /// <summary>
    /// Returns if cutscene can start
    /// </summary>
    /// <returns>It will start if any of the Óliver progression variables that indicates if the cutscene is active is true</returns>
    public override bool CheckStartConditions()
    {
        return (location == SetLocation.Corridor2 && Oliver.pcData.corridor2InitialCutsceneActive) || (location == SetLocation.EmployeeZone && Oliver.pcData.employeeZoneInitialCutsceneActive);
    }
}
