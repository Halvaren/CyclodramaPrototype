using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/// <summary>
/// Manages the UI shown at the end of the demo
/// </summary>
public class ThanksForPlayingUI : MonoBehaviour
{
    public TextMeshProUGUI playedTimeLabel;
    public string playedTimeBaseText;

    /// <summary>
    /// Shows the UI, setting the played time to the played time label
    /// </summary>
    /// <param name="playedTime"></param>
    public void ActivateUI(float playedTime)
    {
        gameObject.SetActive(true);
        SetPlayedTime(playedTime);
    }

    /// <summary>
    /// Transform the float time in a readable string
    /// </summary>
    /// <param name="time"></param>
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

    /// <summary>
    /// It is executed when quit button is clicked
    /// </summary>
    public void QuitButton()
    {
        Application.Quit();
    }
}
