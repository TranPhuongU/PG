using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelMenuManager : MonoBehaviour
{
    public static LevelMenuManager instance;

    [SerializeField] Transform buttonContainer;
    [SerializeField] GameObject levelButtonPrefab;   // Prefab LevelButton
    [SerializeField] LevelDatabase database;

    [SerializeField] GameObject levelPanel;
    [SerializeField] GameObject shopPanel;
    [SerializeField] GameObject settingPanel;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] float musicMinDb = -60f;
    [SerializeField] float musicMaxDb = -5f;

    [SerializeField] float sfxMinDb = -60f;
    [SerializeField] float sfxMaxDb = -5f;


    [SerializeField] Text coinText;
    public int coin {  get; set; }

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GameProgress progress = ProgressManager.Load(database);

        LoadVolume();

        coin = PlayerPrefs.GetInt("Coin", 0);

        UpdateCoin();

        int total = database.allLevels.Count;

        for (int i = 0; i < total; i++)
        {
            int levelID = i + 1;

            LevelData dataDB = database.allLevels[i];
            LevelProgress dataSave = progress.levels.Find(l => l.levelID == levelID);

            LevelButton btn = Instantiate(levelButtonPrefab, buttonContainer).GetComponent<LevelButton>();

            btn.SetLevelNumber(levelID);
            btn.UpdateView(dataSave);

            btn.SetClickAction(() =>
            {
                SelectedLevel.levelID = levelID;
                SceneManager.LoadScene("Gameplay");
            });
        }
        levelPanel.SetActive(false);
    }

    public void PLayButton()
    {
        settingPanel.SetActive(false);
        shopPanel.SetActive(false);
        levelPanel.SetActive(true);
    }
    public void ShopButton()
    {
        settingPanel.SetActive(false);
        levelPanel.SetActive(false);
        shopPanel.SetActive(true);
    }

    public void SettingButton()
    {
        shopPanel.SetActive(false);
        levelPanel.SetActive(false);
        settingPanel.SetActive(true);
    }

    public void SetMusicVolume()
    {
        if (musicSlider == null) return;

        float value = musicSlider.value; // 0..1
        float db = Mathf.Lerp(musicMinDb, musicMaxDb, value);

        audioMixer.SetFloat("Music", db);
        PlayerPrefs.SetFloat("musicVolume", value);
    }

    public void SetSFXVolume()
    {
        if (sfxSlider == null) return;

        float value = sfxSlider.value; // 0..1
        float db = Mathf.Lerp(sfxMinDb, sfxMaxDb, value);

        audioMixer.SetFloat("SFX", db);
        PlayerPrefs.SetFloat("sfxVolume", value);
    }

    private void LoadVolume()
    {
        if (musicSlider == null || sfxSlider == null)
            return;

        musicSlider.value = PlayerPrefs.GetFloat("musicVolume");
        sfxSlider.value = PlayerPrefs.GetFloat("sfxVolume");

        SetMusicVolume();
        SetSFXVolume();

    }

    public void UpdateCoin()
    {
        coinText.text = coin.ToString();
    }

}


public static class SelectedLevel
{
    public static int levelID;
}
