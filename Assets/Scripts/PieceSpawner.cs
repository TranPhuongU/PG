using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{

    public static PieceSpawner instance;

    public Board board;
    public GameObject[] piecePrefabs;
    public int spawnHeight = 4;
    public float spawnChance = 0.4f;

    public PreviewRow[] previewPrefabs;

    // 👉 Thêm: list các preview đang spawn để dễ clear
    private List<PreviewRow> activePreviews = new List<PreviewRow>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        FillInitialRows();
        StartCoroutine(InitialBoardSettleRoutine());
        BuildPreviewFromRow0();


    }

    void FillInitialRows()
    {
        for (int y = 0; y < spawnHeight; y++)
        {
            for (int x = 0; x < board.width; x++)
            {
                if (Random.value > spawnChance)
                    continue;

                Piece sample = CreateRandomPiece();

                if (board.CanPlace(sample, x, y))
                {
                    board.PlacePieceAt(sample, x, y);
                }
                else
                {
                    Destroy(sample.gameObject);
                }

            }
        }
    }

    public void SpawnTopBufferRow()
    {

        for (int x = 0; x < board.width; x++)
        {
            if (Random.value > spawnChance)
                continue;

            Piece sample = CreateRandomPiece();

            if (board.CanPlace(sample, x, 0))
            {
                board.PlacePieceAt(sample, x, 0);
            }
            else
            {
                Destroy(sample.gameObject);
            }

        }

        BuildPreviewFromRow0();


    }

    Piece CreateRandomPiece()
    {
        int index = Random.Range(0, piecePrefabs.Length);
        GameObject obj = Instantiate(piecePrefabs[index]);
        return obj.GetComponent<Piece>();
    }

    // ✨ Cho board rơi & clear ngay khi bắt đầu game
    IEnumerator InitialBoardSettleRoutine()
    {
        board.isResolving = true;

        bool chainContinues = true;

        while (chainContinues)
        {
            bool moved = board.ApplyFullGravitySmooth();

            if (moved)
                yield return new WaitForSeconds(0.12f);

            board.ClearAndCollapseLines();

            chainContinues = moved;

            yield return null;
        }

        board.isResolving = false;
    }

    void ClearPreviewRow()
    {
        foreach (var pr in activePreviews)
        {
            if (pr != null)
                Destroy(pr.gameObject);
        }
        activePreviews.Clear();
    }

    void BuildPreviewFromRow0()
    {
        ClearPreviewRow();

        int y = 0; // hàng buffer trong grid

        for (int x = 0; x < board.width; x++)
        {
            Piece p = board.grid[x, y];
            if (p == null) continue;

            // Lấy prefab preview tương ứng loại piece
            int index = (int)p.type - 1;        // PieceType.One = 1 → index 0
            PreviewRow prefab = previewPrefabs[index];
            if (prefab == null) continue;

            PreviewRow pr = Instantiate(prefab);

            // Lấy vị trí X giống Piece, chỉ đổi Y thành yPosition của preview
            Vector3 pos = p.transform.position;
            pos.y = prefab.yPosition;
            pr.transform.position = pos;

            activePreviews.Add(pr);
        }
    }


}
