using System.Collections;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{

    public static PieceSpawner instance;

    public Board board;
    public GameObject[] piecePrefabs;

    public int spawnWidth = 12;
    public int spawnHeight = 4;
    public float spawnChance = 0.4f;

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        SpawnArea();
        StartCoroutine(InitialSettleRoutine());

    }

    void SpawnArea()
    {
        for (int y = 0; y < spawnHeight; y++)
        {
            for (int x = 0; x < board.width; x++)
            {
                if (Random.value > spawnChance)
                    continue;

                Piece sample = CreateSamplePiece();

                if (board.CanPlace(sample, x, y))
                {
                    board.PlacePiece(sample, x, y);
                }
                else
                {
                    Destroy(sample.gameObject);
                }

            }
        }
    }

    public void SpawnAreaY0()
    {

        for (int x = 0; x < board.width; x++)
        {
            if (Random.value > spawnChance)
                continue;

            Piece sample = CreateSamplePiece();

            if (board.CanPlace(sample, x, 0))
            {
                board.PlacePiece(sample, x, 0);
            }
            else
            {
                Destroy(sample.gameObject);
            }

        }

    }

    Piece CreateSamplePiece()
    {
        int index = Random.Range(0, piecePrefabs.Length);
        GameObject obj = Instantiate(piecePrefabs[index]);
        return obj.GetComponent<Piece>();
    }

    // ✨ Cho board rơi & clear ngay khi bắt đầu game
    IEnumerator InitialSettleRoutine()
    {
        board.isResolving = true;

        bool chainContinues = true;

        while (chainContinues)
        {
            bool moved = board.ApplyFullGravity();

            if (moved)
                yield return new WaitForSeconds(0.12f);

            board.CheckAndClearLines();

            chainContinues = moved;

            yield return null;
        }

        board.isResolving = false;
    }
}
