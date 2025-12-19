using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(RectXformMover))]
public class MessageWindow : MonoBehaviour
{
    public Image messageImage;
    public Text messageText;
    public Text buttonText;

    public Sprite loseIcon;
    public Sprite winIcon;
    public Sprite goalIcon;

    public Sprite goalCompleteIcon;
    public Sprite goalFailedIcon;

    public Image goalImage;
    public Text goalText;

    public GameObject goalObject;

    public Action onStartPressed;

    public void OnButtonStartPressed()
    {
        onStartPressed?.Invoke();
    }

  


    public void ShowMessage(Sprite sprite = null, string message = "", string buttonMsg = "start")
    {
        if (messageImage != null)
        {
            messageImage.sprite = sprite;
        }

        if (messageText != null)
        {
            messageText.text = message;
        }

        if (buttonText != null)
        {
            buttonText.text = buttonMsg;
        }
    }

    public void ShowScoreMessage(int scoreGoal)
    {
        string message = "score goal \n " + scoreGoal.ToString();
        ShowMessage(goalIcon, message, "start");
    }

    public void ShowWinMessage()
    {
        goalObject.SetActive(true);

        ShowMessage(winIcon, "level\ncomplete", "ok");
    }
    public void ShowLoseMessage()
    {
        goalObject.SetActive(true);

        ShowMessage(loseIcon, "level\nfailed", "ok");
    }

    public void ShowGoal(string caption = "", Sprite icon = null)
    {
        if (caption != "")
        {
            ShowGoalCaption(caption);
        }

        if (icon != null)
        {
            ShowGoalImage(icon);
        }
    }

    public void ShowGoalCaption(string caption = "")
    {
        if (goalText != null)
        {
            goalText.text = caption;
        }
    }

    public void ShowGoalImage(Sprite icon = null)
    {
        if (goalIcon != null)
        {
            goalImage.gameObject.SetActive(true);
            goalImage.sprite = icon;
        }

        if (icon == null)
        {
            goalImage.gameObject.SetActive(false);
        }
    }
}
