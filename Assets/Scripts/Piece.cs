using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PieceType
{
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
}

public enum PieceColor
{
    Red,
    Green,
    Orange,
}

public enum PieceBooster
{
    OnePiece,
    ColorPiece,
    ReplacePiece,
    BombPiece,
    None
}

public class Piece : MonoBehaviour
{
    public enum InterpType
    {
        Linear,
        EaseOut,
        EaseIn,
        SmoothStep,
        SmootherStep
    }

    public int score;

    public InterpType interpolation = InterpType.SmootherStep;

    public PieceType type = PieceType.One;

    public PieceColor color = PieceColor.Red; 

    public PieceBooster booster = PieceBooster.None;

    public int rootX;
    public int rootY;

    public GameObject framePrefab;

    public List<Vector2Int> cells = new List<Vector2Int>();

    Coroutine moveCoroutine;



    private void Awake()
    {
        GenerateCells();
    }
    public void GenerateCells()
    {
        cells.Clear();

        int len = (int)type;
        int half = len / 2;

        if (len % 2 == 1)
        {
            // Lẻ 1,3,5 → pivot nằm đúng tâm
            for (int i = -half; i <= half; i++)
                cells.Add(new Vector2Int(i, 0));
        }
        else
        {
            // Chẵn 2,4 → pivot nằm giữa 2 cell
            for (int i = -half; i <= half - 1; i++)
                cells.Add(new Vector2Int(i, 0));
        }
    }


    public List<Vector2Int> GetOccupiedCells()
    {
        List<Vector2Int> result = new List<Vector2Int>();

        foreach (var c in cells)
        {
            result.Add(new Vector2Int(rootX + c.x, rootY + c.y));
        }

        return result;
    }

    public void SetRoot(int x, int y)
    {
        rootX = x;
        rootY = y;
    }

    public void MoveSmooth(int rootX, int rootY, float timeToMove)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(rootX, rootY, timeToMove));
    }

    IEnumerator MoveRoutine(int targetRootX, int targetRootY, float timeToMove)
    {
        Vector3 startPos = transform.position;
        int len = cells.Count;
        float offset = (len % 2 == 0) ? -0.5f : 0f;

        Vector3 endPos = new Vector3(
            targetRootX + offset,
            targetRootY,
            0
        );


        float elapsed = 0f;
        float t;

        while (elapsed < timeToMove)
        {
            elapsed += Time.deltaTime;
            t = Mathf.Clamp01(elapsed / timeToMove);

            switch (interpolation)
            {
                case InterpType.EaseOut:
                    t = Mathf.Sin(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.EaseIn:
                    t = 1 - Mathf.Cos(t * Mathf.PI * 0.5f);
                    break;
                case InterpType.SmoothStep:
                    t = t * t * (3 - 2 * t);
                    break;
                case InterpType.SmootherStep:
                    t = t * t * t * (t * (t * 6 - 15) + 10);
                    break;
            }

            transform.position = Vector3.Lerp(startPos, endPos, t);
            yield return null;
        }

        transform.position = endPos;
        moveCoroutine = null;
    }

    public void ActiveFrame(bool state)
    {
        framePrefab.SetActive(state);
    }
}
