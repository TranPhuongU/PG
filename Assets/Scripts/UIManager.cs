using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager instance;

    public MessageWindow messageWindow;
    public Image progressBar;
    public ScoreMeter scoreMeter;
    private int currentDisplayScore = 0;
    private Coroutine scoreRoutine;


    [SerializeField] Text onePieceBoosterAmountText;
    [SerializeField] Text colorPieceBoosterAmountText;
    [SerializeField] Text replacePieceBoosterAmountText;
    [SerializeField] Text scoreText;
    private void Awake()
    {
        instance = this;

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

    }
    private void GameStateChangedCallBack(GameManager.GameState _gameState)
    {
        if(_gameState == GameManager.GameState.Lose)
        {
            messageWindow.GetComponent<RectXformMover>().MoveOn();
        }
        else if(_gameState == GameManager.GameState.Win)
        {
            messageWindow.GetComponent<RectXformMover>().MoveOn();
            ShowWinScreen();
        }

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

    void ShowWinScreen()
    {

        messageWindow.GetComponent<RectXformMover>().MoveOn();
        messageWindow.ShowWinMessage();

        string scoreStr = "you scored\n" + GameManager.instance.currentScore.ToString() + "points!";
        UIManager.instance.messageWindow.ShowGoalCaption(scoreStr, 0, 70);

        messageWindow.ShowGoalImage(messageWindow.goalCompleteIcon);


        if (SoundManager.instance != null)
        {
            SoundManager.instance.PlayWinSound();
        }
    }

}
