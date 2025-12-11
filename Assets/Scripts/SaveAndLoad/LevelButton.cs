using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LevelButton : MonoBehaviour
{
    public Text levelNumberText;

    private Button button;
    private Image bg;

    public Sprite unlockedSprite;
    public Sprite lockedSprite;

    public Sprite starUnlockedSprite;
    public Sprite starLockedSprite;

    [Header("Gán 3 sao theo thứ tự trái → phải")]
    public GameObject[] stars;



    void Awake()
    {
        button = GetComponent<Button>();
        bg = GetComponent<Image>();
    }

    public void UpdateView(LevelProgress data)
    {
        bool unlocked = data.unlocked;

        bg.sprite = unlocked ? unlockedSprite : lockedSprite;
        button.interactable = unlocked;

        // Hiển thị sao
        for (int i = 0; i < stars.Length; i++)
        {
            Image img = stars[i].GetComponent<Image>();

            img.sprite = (data.stars >= i + 1)
                ? starUnlockedSprite
                : starLockedSprite;
        }

    }

    public void SetClickAction(System.Action callback)
    {
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(() => callback());
    }

    public void SetLevelNumber(int number)
    {
        if (levelNumberText != null)
            levelNumberText.text = number.ToString();
    }

}
