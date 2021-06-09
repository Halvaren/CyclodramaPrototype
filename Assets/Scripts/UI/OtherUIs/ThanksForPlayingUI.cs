using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ThanksForPlayingUI : MonoBehaviour
{
    public TextMeshProUGUI playedTimeLabel;
    public string playedTimeBaseText;

    public void ActivateUI(float playedTime)
    {
        gameObject.SetActive(true);
        SetPlayedTime(playedTime);
    }

    public void SetPlayedTime(float time)
    {
        int intTime = (int)time;
        int hours = intTime / 3600;
        int minutes = (intTime - (hours * 3600)) / 60;
        int seconds = intTime - (hours * 3600) - (minutes * 60);

        string hoursString = hours < 10 ? "0" + hours : hours.ToString();
        string minutesString = minutes < 10 ? "0" + minutes : minutes.ToString();
        string secondsString = seconds < 10 ? "0" + seconds : seconds.ToString();

        playedTimeLabel.text = playedTimeBaseText + hoursString + ":" + minutesString + ":" + secondsString;
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
