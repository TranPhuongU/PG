using UnityEngine;
using UnityEngine.UI;

public class UIBooster : MonoBehaviour
{
    [Header("Booster Info")]
    [SerializeField] private PieceBooster boosterType;
    [SerializeField] private string description;

    [Header("UI")]
    [SerializeField] private Text detailText;
    [SerializeField] private Text amountBooster;

    [SerializeField] private int price;

    private void Start()
    {
        // Text mô tả
        if (detailText != null)
            detailText.text = description;

        // Text số lượng
        UpdateAmount();
    }

    public void UpdateAmount()
    {
        int amount = PlayerPrefs.GetInt(boosterType.ToString(), 0);
        amountBooster.text = amount.ToString();
    }
    public void BuyButton()
    {
        if(LevelMenuManager.instance.coin >= price)
        {
            int amount = PlayerPrefs.GetInt(boosterType.ToString(), 0);
            amount++;
            amountBooster.text = amount.ToString();
            PlayerPrefs.SetInt(boosterType.ToString(), amount);
            LevelMenuManager.instance.coin -= price;
            LevelMenuManager.instance.UpdateCoin();

            PlayerPrefs.SetInt("Coin", LevelMenuManager.instance.coin);
        }
    }
}
