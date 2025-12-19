using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public ScreenFader screenFader;
    public MessageWindow messageWindow;
    public Image progressBar;
    public ScoreMeter scoreMeter;
    private int currentDisplayScore = 0;
    private Coroutine scoreRoutine;

    bool clicked = false;

    [SerializeField] Text onePieceBoosterAmountText;
    [SerializeField] Text colorPieceBoosterAmountText;
    [SerializeField] Text replacePieceBoosterAmountText;
    [SerializeField] Text scoreText;

    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;

    [SerializeField] float musicMinDb = -60f;
    [SerializeField] float musicMaxDb = -5f;

    [SerializeField] float sfxMinDb = -60f;
    [SerializeField] float sfxMaxDb = -5f;


    [SerializeField] GameObject settingPanel;

    private void Awake()
    {
        instance = this;

        if (messageWindow != null)
        {
            messageWindow.gameObject.SetActive(true);
        }

        if (screenFader != null)
        {
            screenFader.gameObject.SetActive(true);
        }

        GameManager.instance.onGameStateChanged += GameStateChangedCallBack;
    }
    private void OnDestroy()
    {
        GameManager.instance.onGameStateChanged -= GameStateChangedCallBack;

    }

    private void Start()
    {
        UpdateBoosterTexts();
        UpdateScore(GameManager.instance.currentScore);

        if (PlayerPrefs.HasKey("musicVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
            SetSFXVolume();
        }

        settingPanel.SetActive(false);

    }
    private void GameStateChangedCallBack(GameState _gameState)
    {
        //if(_gameState == GameState.Lose)
        //{
        //    messageWindow.GetComponent<RectXformMover>().MoveOn();
        //    ShowMessageWindow(_gameState);

        //}
        //else if(_gameState == GameState.Win)
        //{
        //    messageWindow.GetComponent<RectXformMover>().MoveOn();
        //    ShowMessageWindow(_gameState);
        //}

        ShowMessageWindow(_gameState);
       
    }


    public void ResetGame()
    {
        SceneManager.LoadScene(0);
    }
    public float smoothTime = 0.3f; // thời gian animation

    private Coroutine fillRoutine;

    public void AnimateFillTo(int currentScore, int targetScore)
    {
        float targetFill = (float)currentScore / targetScore;

        // nếu coroutine trước đang chạy thì dừng nó để tránh “đánh nhau”
        if (fillRoutine != null)
            StopCoroutine(fillRoutine);

        fillRoutine = StartCoroutine(FillTo(targetFill));
    }

    private IEnumerator FillTo(float target)
    {
        float start = progressBar.fillAmount;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / smoothTime;
            progressBar.fillAmount = Mathf.Lerp(start, target, t);

            yield return null; // đợi 1 frame
        }

        progressBar.fillAmount = target; // chốt lại chính xác
    }

    public void UpdateBoosterTexts()
    {
        onePieceBoosterAmountText.text = GameManager.instance.onePieceBoosterAmount.ToString();
        colorPieceBoosterAmountText.text = GameManager.instance.colorPieceBoosterAmount.ToString();
        replacePieceBoosterAmountText.text = GameManager.instance.replacePieceBoosterAmount.ToString();
    }

    public void UpdateScore(int targetScore)
    {
        if (scoreRoutine != null)
            StopCoroutine(scoreRoutine);

        scoreRoutine = StartCoroutine(AnimateScore(targetScore));
    }


    private IEnumerator AnimateScore(int targetScore)
    {
        float duration = 0.3f; // thời gian chạy animation
        float t = 0f;

        int startScore = currentDisplayScore;

        while (t < 1f)
        {
            t += Time.deltaTime / duration;

            currentDisplayScore = (int)Mathf.Lerp(startScore, targetScore, t);
            scoreText.text = currentDisplayScore.ToString();

            yield return null;
        }

        // chốt lại chính xác
        currentDisplayScore = targetScore;
        scoreText.text = targetScore.ToString();
    }

    void ShowMessageWindow(GameState gameState)
    {
        string scoreStr = "you scored\n" + GameManager.instance.currentScore.ToString() + " points!";


        switch (gameState)
        {
            case GameState.Win:
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowWinMessage();

                UIManager.instance.messageWindow.ShowGoalCaption(scoreStr);

                messageWindow.ShowGoalImage(messageWindow.goalCompleteIcon);
                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlayWinSound();
                }

                // tăng level lên 1
                SelectedLevel.levelID++; 

                // nếu vượt quá số level thì giữ nguyên hoặc quay về 1 tùy bạn
                if (SelectedLevel.levelID > GameManager.instance.database.allLevels.Count)
                    SelectedLevel.levelID = GameManager.instance.database.allLevels.Count; // hoặc =1 nếu muốn loop
                break;
            case GameState.Lose:
                messageWindow.GetComponent<RectXformMover>().MoveOn();
                messageWindow.ShowLoseMessage();

                UIManager.instance.messageWindow.ShowGoalCaption(scoreStr);

                messageWindow.ShowGoalImage(messageWindow.goalCompleteIcon);
                if (SoundManager.instance != null)
                {
                    SoundManager.instance.PlayLoseSound();
                }
                break;
        }
        PieceSpawner.instance.board.isResolving = true;

    }

    public void OnMainButtonClicked()
    {

        if (clicked) return;   // ⛔ chặn click tiếp
        clicked = true;

        GameManager gm = GameManager.instance;

        switch (gm.GetCurrentState())
        {
            case GameState.Intro:
                // READY / START GAME
                messageWindow.OnButtonStartPressed();
                break;

            case GameState.Win:
            case GameState.Lose:
                // RELOAD SCENE
                SceneManager.LoadScene("Gameplay");
                break;
        }
        clicked = false;

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


    public void SettingButton()
    {
        settingPanel.SetActive(!settingPanel.activeSelf);
    }

}
