using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;
public enum GameState
{
    Menu,
    Game,
    Win,
    Lose,
    Intro
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public BoardBGScaler[] boardBGScalers;

    public int scoreStars;

    public int currentScore;
    public int[] scoreGoals;

    public bool m_isReadyToBegin = false;

    public int coin;

    GameState gameState;

    public int onePieceBoosterAmount = 10;
    public int colorPieceBoosterAmount = 10;
    public int replacePieceBoosterAmount = 10;

    public LevelDatabase database;  // gán từ Inspector

    public Action<GameState> onGameStateChanged;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        instance = this;
        Application.targetFrameRate = 90;

        Board.onPieceCleared += HandleScoreEvent;
    }

    private void OnDestroy()
    {
        Board.onPieceCleared -= HandleScoreEvent;

    }

    private void Start()
    {
        currentScore = 0;
        LoadLevel(SelectedLevel.levelID);

        SetGameState(GameState.Intro);

        StartCoroutine(ShowIntroRoutine());

        // UIManager.instance.scoreMeter.SetupStars(this);
        UIManager.instance.scoreMeter.SetupVStars(this);

    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.D))
        {
            ProgressManager.DeleteSave();
        }
    }

    public void SetGameState(GameState _gameState)
    {
        gameState = _gameState;
        onGameStateChanged?.Invoke(_gameState);
    }

    public GameState GetCurrentState()
    {
        return gameState;
    }
    public bool IsWiner()
    {
        int maxScored = scoreGoals[scoreGoals.Length - 1];

        if(currentScore >= maxScored)
        {
            coin += 100;
            return true;
        }

        return false;
    }

    public bool IsGameOver()
    {
        if(currentScore >= scoreGoals[0])
        {
            return true;
        }
        return false;
    }

    public void SetScore(int _score)
    {
        currentScore += _score;
    }

    public int UpdateScore(int _score)
    {
        for (int i = 0; i < scoreGoals.Length; i++)
        {
            if(_score < scoreGoals[i])
            {
                return i;
            }
        }

        return scoreGoals.Length;
    }

    public void UpdateScoreStars(int _score)
    {
        scoreStars = UpdateScore(_score);
    }

    void HandleScoreEvent(int pieceScore, Piece piece)
    {
        // ⛔ Game đã kết thúc → bỏ qua toàn bộ scoring
        if (gameState != GameState.Game)
            return;

        SetScore(pieceScore);

        SoundManager.instance.PlayClearPieceSound();
        UpdateScoreStars(currentScore);
        UIManager.instance.UpdateScore(currentScore);

        UIManager.instance.scoreMeter.UpdateScoreMeter(scoreStars);
        UIManager.instance.AnimateFillTo(currentScore, scoreGoals[2]);

        if(piece != null)
        {
            if(piece.booster == PieceBooster.OnePiece)
            {
                onePieceBoosterAmount++;
            }
            if (piece.booster == PieceBooster.ColorPiece)
            {
                colorPieceBoosterAmount++;
            }
            if (piece.booster == PieceBooster.ReplacePiece)
            {
                replacePieceBoosterAmount++;
            }
        }
        UIManager.instance.UpdateBoosterTexts();


        if (IsWiner())
        {
            SaveProgress();
            SetGameState(GameState.Win);
        }
    }


    public void SaveProgress()
    {
        PlayerPrefs.SetInt("OnePiece",onePieceBoosterAmount);
        PlayerPrefs.SetInt("ColorPiece",colorPieceBoosterAmount);
        PlayerPrefs.SetInt("ReplacePiece",replacePieceBoosterAmount);
        PlayerPrefs.SetInt("Coin", coin);

        // Tách số từ "LevelX"
        int levelID = SelectedLevel.levelID;

        // Lấy sao đã đạt
        int stars = scoreStars;

        // Lấy điểm
        int score = currentScore;

        // Gọi save JSON
        ProgressManager.SetLevelResult(levelID, stars, score,database);
    }
    void LoadLevel(int id)
    {
        LevelData data = database.allLevels.Find(l => l.levelID == id);

        if (data == null)
        {
            Debug.LogError($"Không tìm thấy LevelData cho level: {id}");
            return;
        }

        scoreGoals = data.scoreGoals;

        // 2) Spawn layout nếu có
        if (data.layoutPrefab != null)
        {
            GameObject gameObjectBoard = Instantiate(data.layoutPrefab);

            Board board = gameObjectBoard.GetComponent<Board>();

            board.InitBoard(data.width, data.height); // ✔ KHỞI TẠO ĐÚNG THỜI ĐIỂM
            board.isResolving = true;

        }

        onePieceBoosterAmount = PlayerPrefs.GetInt("OnePiece");
        colorPieceBoosterAmount = PlayerPrefs.GetInt("ColorPiece");
        replacePieceBoosterAmount = PlayerPrefs.GetInt("ReplacePiece");
    }

    IEnumerator ShowIntroRoutine()
    {
        // 2) Message MoveOn
        UIManager.instance.messageWindow.GetComponent<RectXformMover>().MoveOn();
        UIManager.instance.messageWindow.ShowScoreMessage(scoreGoals[scoreGoals.Length - 1]);

        // 3) Chờ player nhấn Start

        UIManager.instance.messageWindow.onStartPressed = () => m_isReadyToBegin = true;

        while (!m_isReadyToBegin)
            yield return null;

        // 4) Bắt đầu fade OFF
        UIManager.instance.screenFader.FadeOff();
        UIManager.instance.messageWindow.GetComponent<RectXformMover>().MoveOff();

        yield return new WaitForSeconds(0.4f);

        // 5) Bắt đầu game
        SetGameState(GameState.Game);

        StartGameplay();
    }
    void StartGameplay()
    {
        Board board = FindFirstObjectByType<Board>();

        if (board != null)
        {
            // Spawn mở màn: piece từ trên rơi xuống
            StartCoroutine(PieceSpawner.instance.IntroRiseAnimation());

        }
    }


}
