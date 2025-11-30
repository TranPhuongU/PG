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

    public InterpType interpolation = InterpType.SmootherStep;

    Board board;

    Vector3 mouseDownWorld;
    float cellThreshold = 1f;

    public PieceType type = PieceType.One;

    public int rootX;
    public int rootY;

    public List<Vector2Int> cells = new List<Vector2Int>();

    bool m_isMoving = false;


    private void Awake()
    {
        GenerateCells();
        board = FindObjectOfType<Board>();
    }

    void GenerateCells()
    {
        cells.Clear();

        int len = (int)type;

        for (int i = 0; i < len; i++)
        {
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
    Coroutine moveCoroutine;

    public void MoveSmooth(int rootX, int rootY, float timeToMove)
    {
        if (moveCoroutine != null)
            StopCoroutine(moveCoroutine);

        moveCoroutine = StartCoroutine(MoveRoutine(rootX, rootY, timeToMove));
    }

    IEnumerator MoveRoutine(int targetRootX, int targetRootY, float timeToMove)
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = new Vector3(
            targetRootX * board.cellSize,
            targetRootY * board.cellSize,
            0);

        float elapsed = 0f;
        float t;

        m_isMoving = true;

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
        m_isMoving = false;

        moveCoroutine = null;
    }


}
