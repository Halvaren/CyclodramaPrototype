using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnacletoBehavior : NPCBehavior
{
    public VIDE_Assign firstTimeConv;

    public AudioClip anacletoTheme;

    public override IEnumerator TalkMethod()
    {
        AudioSource anacletoThemeSource = null;
        if (firstTimeTalk)
        {
            anacletoThemeSource = AudioManager.PlaySound(anacletoTheme, SoundType.ForegroundMusic);
            firstTimeTalk = false;
        }

        AddAnimationLock();
        mainAnimationCallback += ReleaseAnimationLock;
        WakeUp();

        while (animationLocks.Count > 0)
        {
            yield return null;
        }

        mainAnimationCallback -= ReleaseAnimationLock;

        yield return StartCoroutine(_StartConversation(firstTimeConv));
        if (anacletoThemeSource != null) AudioManager.FadeOutSound(anacletoThemeSource, 3f);

        GoSleep();
    }

    public override IEnumerator _BeginDialogue(VIDE_Assign dialogue)
    {
        yield return base._BeginDialogue(dialogue);
    }

    #region Animations

    public void WakeUp()
    {
        Animator.SetTrigger("wakeUp");
    }

    public void GoSleep()
    {
        Animator.SetTrigger("goSleep");
    }

    #endregion
}
