using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InitialCutscene : BasicCutscene
{
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

    public override IEnumerator RunCutscene()
    {
        if(location == SetLocation.Corridor2)
        {
            AudioSource oliverThemeSource = AudioManager.PlaySound(oliverTheme, SoundType.ForegroundMusic);

            yield return new WaitForSeconds(0.5f);

            yield return StartCoroutine(corridor1Door._StartConversation(oliverIntroduction));
            AudioManager.FadeOutSound(oliverThemeSource, 1f, 0.5f);

            yield return StartCoroutine(Yaiza.SpawnFromDoor(true, corridor1Door, Vector3.right, 10f, false));
            yield return StartCoroutine(Yaiza.GoToPoint(yaizaStopPoint.position, true));

            yield return StartCoroutine(Yaiza.MovementController.RotateToDirectionCoroutine(Oliver.transform.position - Yaiza.transform.position));
            yield return StartCoroutine(Yaiza._StartConversation(yaizaToOliver));

            yield return StartCoroutine(Yaiza.GoToDoorAndExit(true, employeeZoneDoor, Vector3.forward, false, 1.5f));

            AudioManager.FadeOutSound(oliverThemeSource, 3f);
            corridor1Door.transitionTrigger.cantGoThrough = true;
            costumeWorkshopDoor.transitionTrigger.cantGoThrough = true;

            Oliver.pcData.corridor2InitialCutsceneActive = false;
        }
        else if(location == SetLocation.EmployeeZone)
        {
            yield return StartCoroutine(Oliver.MovementController.MoveAndRotateToPoint(oliverStopPoint.position, Leon.transform.position));

            AudioSource leonThemeSource = AudioManager.PlaySound(leonTheme, SoundType.ForegroundMusic);
            yield return StartCoroutine(Leon._StartConversation(staffConversation));
            AudioManager.FadeOutSound(leonThemeSource, 3f);

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

            Oliver.pcData.employeeZoneInitialCutsceneActive = false;
        }

        yield return null;
    }

    public override bool CheckStartConditions()
    {
        return (location == SetLocation.Corridor2 && Oliver.pcData.corridor2InitialCutsceneActive) || (location == SetLocation.EmployeeZone && Oliver.pcData.employeeZoneInitialCutsceneActive);
    }
}
