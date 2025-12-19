using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PieceSpawner : MonoBehaviour
{

    public static PieceSpawner instance;

    [HideInInspector]
    public Board board;
    public GameObject[] piecePrefabs;
    public GameObject[] onePiecePrefabsByColor;     // index theo PieceColor enum

    public int spawnHeight = 4;
    public float spawnChance = 0.4f;

    public PreviewRow[] previewPrefabs;

    // 👉 Thêm: list các preview đang spawn để dễ clear
    private List<PreviewRow> activePreviews = new List<PreviewRow>();
    private List<Piece> initialPieces = new List<Piece>();


    private void Awake()
    {
        instance = this;
    }

    private IEnumerator Start()
    {
        board = FindFirstObjectByType<Board>();
        // đợi Board Awake + Start chạy xong
        yield return null;

        FillInitialRows();
        BuildPreviewFromRow0();
    }


    void FillInitialRows()
    {
        for (int y = 0; y < spawnHeight; y++)
        {
            int emptyX = Random.Range(0, board.width);

            int x = 0;
            while (x < board.width)
            {
                if (x == emptyX)
                {
                    x++;
                    continue;
                }

                Piece p = CreateRandomPiece();

                if (!FitsHorizontal(p, x))
                {
                    Destroy(p.gameObject);
                    x++;
                    continue;
                }

                if (!HasSupport(p, x, y))
                {
                    Destroy(p.gameObject);
                    x++;
                    continue;
                }

                if (!board.CanPlace(p, x, y))
                {
                    Destroy(p.gameObject);
                    x++;
                    continue;
                }

                // ❌ không cho piece chiếm emptyX
                if (OccupiesX(p, x, emptyX))
                {
                    Destroy(p.gameObject);
                    x++;
                    continue;
                }


                // đặt vào grid logic
                board.PlacePieceAt(p, x, y);

                // spawn ẩn dưới đáy đúng theo pivot
                Vector3 finalPos = p.GetWorldPos(x, y);

                float startOffset = -6f;
                p.transform.position = new Vector3(finalPos.x, finalPos.y + startOffset, 0);

                // add vào list
                initialPieces.Add(p);




                x += p.cells.Count;
            }
        }
    }



    // Hỗ trợ: chỉ cần 1 cell của piece có support
    bool HasSupport(Piece p, int rx, int ry)
    {
        foreach (var c in p.cells)
        {
            int nx = rx + c.x;
            int ny = ry + c.y;

            // Nếu cell này nằm ngoài grid → bỏ qua (không được tính)
            if (!board.IsInside(nx, ny))
                continue;

            int belowY = ny - 1;

            // Nếu dưới rìa đáy (y < 1) → coi như có support
            if (belowY < 1)
                return true;

            // Nếu vị trí dưới trong grid
            if (board.IsInside(nx, belowY))
            {
                // Chỉ cần 1 cell có support
                if (board.grid[nx, belowY] != null)
                    return true;
            }
        }

        // Không cell nào có support
        return false;
    }
    bool FitsHorizontal(Piece p, int rx)
    {
        foreach (var c in p.cells)
        {
            int nx = rx + c.x;
            if (nx < 0 || nx >= board.width)
                return false;
        }
        return true;
    }

    public void SpawnPieceY0()
    {
        // Đảm bảo hàng y=0 không bao giờ full
        int emptyX = Random.Range(0, board.width);

        int y = 0;
        int x = 0;

        while (x < board.width)
        {
            // Bỏ 1 ô trống bắt buộc
            if (x == emptyX)
            {
                x++;
                continue;
            }

            // Random piece
            Piece p = CreateRandomPiece();
            int len = p.cells.Count;

            // Check horizontal bounds (tránh crash vì pivot âm/dương)
            if (!FitsHorizontal(p, x))
            {
                Destroy(p.gameObject);
                x++;
                continue;
            }

            // Check collision
            if (!board.CanPlace(p, x, y))
            {
                Destroy(p.gameObject);
                x++;
                continue;
            }
            // ❌ không cho piece chiếm emptyX
            if (OccupiesX(p, x, emptyX))
            {
                Destroy(p.gameObject);
                x++;
                continue;
            }


            // Place piece
            board.PlacePieceAt(p, x, y);

            // Skip qua chiều dài piece
            x += len;
        }

        BuildPreviewFromRow0();
    }


    Piece CreateRandomPiece()
    {
        int index = Random.Range(0, piecePrefabs.Length);
        GameObject obj = Instantiate(piecePrefabs[index], board.gameObject.transform);
        return obj.GetComponent<Piece>();
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
    public GameObject GetOnePiecePrefabByColor(PieceColor color)
    {
        return onePiecePrefabsByColor[(int)color];
    }
    // ================== INTRO RISE ANIMATION ==================
    public IEnumerator IntroRiseAnimation()
    {
        float riseSpeed = 0.1f;
        bool done = false;

        while (!done)
        {
            done = true;

            foreach (Piece p in initialPieces)
            {
                if (p == null) continue;

                Vector3 targetPos = p.GetWorldPos(p.rootX, p.rootY);
                Vector3 cur = p.transform.position;

                if (cur.y < targetPos.y)
                {
                    float newY = cur.y + riseSpeed;
                    if (newY > targetPos.y) newY = targetPos.y;

                    p.transform.position = new Vector3(targetPos.x, newY, 0);

                    if (newY < targetPos.y)
                        done = false;
                }
            }

            yield return null;
        }

        // đảm bảo final snap đúng
        foreach (Piece p in initialPieces)
        {
            p.transform.position = p.GetWorldPos(p.rootX, p.rootY);
        }

        BuildPreviewFromRow0();

        // 🔓 mở input sau khi intro hoàn tất
        if (board != null)
        {
            board.isResolving = false;
        }

    }

    bool OccupiesX(Piece p, int rootX, int targetX)
    {
        foreach (var c in p.cells)
        {
            if (rootX + c.x == targetX)
                return true;
        }
        return false;
    }


}
