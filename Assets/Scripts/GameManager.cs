using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public BoardBGScaler[] boardBGScalers;

    public int scoreStars;

    public int currentScore;
    public int[] scoreGoals;

    public enum GameState
    {
        Menu,
        Game,
        Win,
        Lose,
    }

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

       // UIManager.instance.scoreMeter.SetupStars(this);
        UIManager.instance.scoreMeter.SetupVStars(this);

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
        SetScore(pieceScore);
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
        }
    }
}
