using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    Vector2 dragStartScreenPos;
    int lastStepX;
    float pixelsPerCell;



    public GameObject tilePrefab;   // gán từ Inspector
    Tile[,] tiles;

    Piece draggingPiece;
    Tile lastTileOver;

    public int width = 8;
    public int height = 20;

    public Piece[,] grid;
    public bool isResolving = false;
    bool isRaising = false;

    public GameObject frameSquarePrefab;
    private GameObject activeFrame;

    private int dragStartX;
    private int dragStartY;

    private const float PIECE_FADE_DURATION = 0.2f; // phải khớp với DestroyPieceRoutine


    private bool lastFullLineResult = false;


    public static Action<int, Piece> onPieceCleared;

    void Start()
    {
        if (grid == null)
            return; // đợi InitBoard

        activeFrame = Instantiate(frameSquarePrefab, transform);
        activeFrame.SetActive(false);

        pixelsPerCell =
    Camera.main.WorldToScreenPoint(Vector3.right).x -
    Camera.main.WorldToScreenPoint(Vector3.zero).x;

    }

    void Update()
    {
        if (draggingPiece == null) return;
        if (isResolving) return;
        if (!Input.GetMouseButton(0)) return;

        float deltaX = Input.mousePosition.x - dragStartScreenPos.x;

        int stepX = Mathf.FloorToInt(deltaX / pixelsPerCell);
        int moveDelta = stepX - lastStepX;

        if (moveDelta == 0) return;

        int dir = moveDelta > 0 ? 1 : -1;

        if (MovePieceBy(draggingPiece, dir, 0))
        {
            lastStepX += dir;
        }
    }

    public void InitBoard(int w, int h)
    {
        width = w;
        height = h;

        grid = new Piece[width, height];

        BuildTiles();

        // GỌI BACKGROUND SCALER
        for (int i = 0; i < GameManager.instance.boardBGScalers.Length; i++)
        {
            GameManager.instance.boardBGScalers[i].FitBoard(width);
        }
    }

    void BuildTiles()
    {
        tiles = new Tile[width, height];

        for (int y = 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x, y, 0);
                GameObject tObj = Instantiate(tilePrefab, pos, Quaternion.identity);
                tObj.transform.SetParent(transform);

                Tile t = tObj.GetComponent<Tile>();
                t.Init(x, y, this);

                tiles[x, y] = t;
            }
        }
    }

    public bool IsInside(int x, int y)
    {
        return x >= 0 && x < width && y >= 0 && y < height;
    }

    public bool CanPlace(Piece p, int rx, int ry)
    {
        foreach (var c in p.cells)
        {
            int x = rx + c.x;
            int y = ry + c.y;

            if (!IsInside(x, y)) return false;
            if (grid[x, y] != null && grid[x, y] != p) return false;
        }
        return true;
    }

    public bool PlacePieceAt(Piece piece, int x, int ry)
    {
        if (!CanPlace(piece, x, ry))
            return false;

        RemovePieceFromGrid(piece);

        piece.rootX = x;
        piece.rootY = ry;

        foreach (var c in piece.cells)
            grid[x + c.x, ry + c.y] = piece;

        piece.transform.position = piece.GetWorldPos(x, ry);

        return true;
    }


    public void RemovePieceFromGrid(Piece piece)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == piece)
                {
                    grid[x, y] = null;
                }
    }

    public bool CanPieceMove(Piece p, int dx, int dy)
    {
        foreach (var c in p.cells)
        {
            int nx = p.rootX + c.x + dx;
            int ny = p.rootY + c.y + dy;

            // không cho rơi xuống hàng 0
            if (ny < 1) return false;

            // vẫn cần trong grid
            if (!IsInside(nx, ny)) return false;

            if (grid[nx, ny] != null && grid[nx, ny] != p) return false;
        }
        return true;
    }

    public bool MovePieceBy(Piece p, int dx, int dy)
    {
        if (!CanPieceMove(p, dx, dy))
            return false;

        RemovePieceFromGrid(p);

        p.rootX += dx;
        p.rootY += dy;

        foreach (var c in p.cells)
            grid[p.rootX + c.x, p.rootY + c.y] = p;

        p.MoveSmooth(p.rootX, p.rootY, 0.12f);

        return true;
    }

    public bool CanPieceFall(Piece p)
    {
        foreach (var c in p.cells)
        {
            int ny = p.rootY + c.y - 1;

            // chặn rơi xuống hàng 0
            if (ny < 1) return false;

            int nx = p.rootX + c.x;

            if (grid[nx, ny] != null && grid[nx, ny] != p)
                return false;
        }

        return true;
    }

    public bool ApplyOneStepFall(Piece p) => MovePieceBy(p, 0, -1);

    public bool ApplyFullGravitySmooth()
    {
        if (isRaising) return false;

        bool moved = false;
        bool falling = true;

        while (falling)
        {
            falling = false;
            List<Piece> toMove = new List<Piece>();

            // 1. Gom tất cả piece có thể rơi trong tick này
            for (int y = 1; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Piece p = grid[x, y];
                    if (p != null && CanPieceFall(p) && !toMove.Contains(p))
                    {
                        toMove.Add(p);
                    }
                }
            }

            // 2. Nếu không có piece nào rơi → dừng
            if (toMove.Count == 0)
                break;

            // 3. Xóa vị trí cũ
            foreach (Piece p in toMove)
                RemovePieceFromGrid(p);

            // 4. cập nhật rootY và grid mới
            foreach (Piece p in toMove)
            {
                p.rootY -= 1;
                foreach (var c in p.cells)
                {
                    grid[p.rootX + c.x, p.rootY + c.y] = p;
                }
            }

            // 5. MoveSmooth *đồng bộ*
            foreach (Piece p in toMove)
                p.MoveSmooth(p.rootX, p.rootY, 0.12f);

            moved = true;
            falling = true; // cho tick tiếp theo
        }

        return moved;
    }

    bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
            if (grid[x, y] == null)
                return false;
        return true;
    }

    IEnumerator ClearLineCoroutine(int y)
    {
        HashSet<Piece> pieces = new HashSet<Piece>();

        // gom tất cả piece ở hàng y
        for (int x = 0; x < width; x++)
        {
            if (grid[x, y] != null)
                pieces.Add(grid[x, y]);
        }

        // xóa logic trong grid trước, để hàng này coi như trống
        for (int x = 0; x < width; x++)
        {
            grid[x, y] = null;
        }

        // gọi DestroyPiece → nó sẽ tự fade & Destroy
        foreach (Piece p in pieces)
        {
            DestroyPiece(p);
        }

        // ĐỢI cho fade xong (thời gian phải khớp với DestroyPieceRoutine)
        float fadeDuration = 0.2f;
        yield return new WaitForSeconds(fadeDuration);
    }


    void DropAbove(int clearedY)
    {
        for (int y = clearedY + 1; y < height; y++)  // không đụng y = 0
        {
            for (int x = 0; x < width; x++)
            {
                Piece p = grid[x, y];
                if (p != null)
                    while (CanPieceFall(p))
                        ApplyOneStepFall(p);
            }
        }
    }

    public IEnumerator ResolveAfterMove()
    {
        isResolving = true;

        bool changed;

        // ============================
        // PHA 1: GIẢI QUYẾT COMBO TRƯỚC KHI DÂNG
        // ============================
        do
        {
            // 1. Cho tất cả rơi xuống hết mức có thể
            while (ApplyFullGravitySmooth())
            {
                yield return new WaitForSeconds(.3f);
            }

            // 2. Clear line + DropAbove
            changed = false;

            yield return StartCoroutine(HandleFullLineCoroutine());
            changed = lastFullLineResult;
            if (changed)
            {
                // cho player thấy hiệu ứng clear + rơi
                yield return new WaitForSeconds(0.1f);
            }

        } while (changed);  // lặp tới khi KHÔNG còn line nào full nữa

        // ============================
        // PHA 2: DÂNG HÀNG
        // ============================
        yield return StartCoroutine(RaiseBoardSmooth());

        // Gravity sau khi dâng (nếu luật cho phép)
        while (ApplyFullGravitySmooth())
        {
            yield return new WaitForSeconds(0.3f);
        }

        // Spawn hàng buffer y = 0
        PieceSpawner.instance.SpawnPieceY0();

        do
        {
            // rơi hết sau khi dâng + spawn
            while (ApplyFullGravitySmooth())
            {
                yield return new WaitForSeconds(0.05f);
            }

            changed = false;
            yield return StartCoroutine(HandleFullLineCoroutine());
            changed = lastFullLineResult;
            if (changed)
            {
                yield return new WaitForSeconds(0.1f);
            }

        } while (changed);

        if (CheckGameOver())
        {
            GameManager.instance.SaveProgress();
            if (GameManager.instance.IsGameOver())
            {
                GameManager.instance.SetGameState(GameState.Win);
            }
            else
            {
                GameManager.instance.SetGameState(GameState.Lose);
            }
            yield break;
        }

        if (GameManager.instance.GetCurrentState() == GameState.Game)
        {
            isResolving = false;
        }
    }

    IEnumerator HandleFullLineCoroutine()
    {
        bool changed = false;

        for (int y = 1; y < height; y++)
        {
            if (IsLineFull(y))
            {
                // 1) Fade & xoá line
                yield return StartCoroutine(ClearLineCoroutine(y));

                // 2) Sau khi fade xong mới cho rơi
                DropAbove(y);

                // 3) Kiểm tra lại cùng hàng (vì các hàng trên đã rơi xuống)
                y--;

                changed = true;
            }
        }

        lastFullLineResult = changed;
    }


    public void HandlePressTile(Tile tile)
    {
        if (isResolving) return;

        draggingPiece = grid[tile.xIndex, tile.yIndex];
        if (draggingPiece == null) return;

        dragStartX = draggingPiece.rootX;
        dragStartY = draggingPiece.rootY;

        dragStartScreenPos = Input.mousePosition;
        lastStepX = 0;

        ShowPieceFrame(draggingPiece);
        draggingPiece.ActiveFrame(true);
    }


    void ShowPieceFrame(Piece p)
    {
        // Position (root cell)
        activeFrame.transform.position = p.transform.position;

        // Scale theo chiều dài piece
        float length = p.cells.Count;       // 1,2,3,4,5...
        activeFrame.GetComponent<SpriteRenderer>().size = new Vector2(length, 1f);

        activeFrame.SetActive(true);
    }

    public void HandleDragTile(Tile tile)
    {
        if (isResolving) return;
        if (draggingPiece == null) return;

        // 🔴 FIX QUAN TRỌNG
        if (lastTileOver == null)
        {
            lastTileOver = tile;
            return;
        }

        int deltaX = tile.xIndex - lastTileOver.xIndex;

        if (deltaX > 0)
        {
            if (MovePieceBy(draggingPiece, +1, 0))
            {
                lastTileOver = tile;
            }
        }
        else if (deltaX < 0)
        {
            if (MovePieceBy(draggingPiece, -1, 0))
            {
                lastTileOver = tile;
            }
        }
    }


    public void HandleReleaseTile()
    {
        if (draggingPiece == null) return;

        if (draggingPiece.rootX == dragStartX &&
            draggingPiece.rootY == dragStartY)
        {
            activeFrame?.SetActive(false);
            draggingPiece.ActiveFrame(false);
            draggingPiece = null;
            return;
        }

        StartCoroutine(ResolveAfterMove());

        activeFrame.SetActive(false);
        draggingPiece.ActiveFrame(false);
        draggingPiece = null;
    }


    public IEnumerator RaiseBoardSmooth()
    {
        isRaising = true;

        Piece[,] newGrid = new Piece[width, height];

        // danh sách move cho đồng bộ
        List<Piece> piecesToMove = new List<Piece>();

        // bước 1: cập nhật grid logic trước
        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Piece p = grid[x, y];
                if (p == null) continue;

                int newY = y + 1;
                if (newY >= height) newY = height - 1;

                newGrid[x, newY] = p;

                p.rootY = newY;   // cập nhật logic ngay
                piecesToMove.Add(p);
            }
        }

        // thay grid bằng grid mới
        grid = newGrid;

        // bước 2: TẤT CẢ MoveSmooth cùng lúc
        foreach (Piece p in piecesToMove)
        {
            p.MoveSmooth(p.rootX, p.rootY, 0.15f);
        }

        // chờ animation kết thúc
        yield return new WaitForSeconds(0.3f);

        isRaising = false;
    }

    public void RemovePieceAt(int x, int y)
    {
        Piece p = grid[x, y];
        if (p == null) return;

        StartCoroutine(RemoveSinglePieceRoutine(p));
    }

    private IEnumerator RemoveSinglePieceRoutine(Piece p)
    {
        // Xóa logic + bắt đầu fade
        DestroyPiece(p);

        // Đợi fade xong (thời gian phải khớp với DestroyPieceRoutine)
        yield return new WaitForSeconds(PIECE_FADE_DURATION);

        // Sau đó mới cho board xử lý rơi / clear / raise
        if (!IsBoardEmpty())
        {
            yield return StartCoroutine(ResolveAfterBooster());
        }
        else
        {
            yield return StartCoroutine(ResolveAfterMove());
        }
    }


    public void RemoveAllSameColor(Piece target)
    {
        if (target == null) return;

        StartCoroutine(RemoveAllSameColorRoutine(target));
    }

    private IEnumerator RemoveAllSameColorRoutine(Piece target)
    {
        List<Piece> toRemove = new List<Piece>();

        // Tìm tất cả piece cùng màu
        for (int y = 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Piece p = grid[x, y];
                if (p != null && p.color == target.color)
                {
                    if (!toRemove.Contains(p))
                        toRemove.Add(p);
                }
            }
        }

        // Gọi DestroyPiece cho từng cái → nó tự fade + xoá logic
        foreach (Piece p in toRemove)
        {
            DestroyPiece(p);
        }

        // Đợi tất cả fade xong
        yield return new WaitForSeconds(PIECE_FADE_DURATION);

        // Rồi mới resolve board
        if (!IsBoardEmpty())
        {
            yield return StartCoroutine(ResolveAfterBooster());
        }
        else
        {
            yield return StartCoroutine(ResolveAfterMove());
        }
    }



    public void ReplacePieceAt(Piece piece)
    {
        if (piece == null || piece.type == PieceType.One) return;

        // 1) Lấy tất cả cell mà piece đang chiếm
        List<Vector2Int> cells = piece.GetOccupiedCells();
        PieceColor color = piece.color;

        // 2) Xóa piece cũ
        RemovePieceFromGrid(piece);
        Destroy(piece.gameObject);

        // 3) Spawn piece One vào từng cell
        foreach (var cell in cells)
        {
            SpawnOneAt(cell.x, cell.y, color);
        }

        // 4) Cho hệ thống xử lý rơi + clear
        StartCoroutine(ResolveAfterBooster());

        GameManager.instance.replacePieceBoosterAmount--;
        UIManager.instance.UpdateBoosterTexts();
    }

    private void SpawnOneAt(int x, int y, PieceColor color)
    {
        GameObject prefab = PieceSpawner.instance.GetOnePiecePrefabByColor(color);

        GameObject obj = Instantiate(prefab);
        Piece p = obj.GetComponent<Piece>();

        p.type = PieceType.One;
        p.color = color;
        p.GenerateCells();

        p.rootX = x;
        p.rootY = y;
        p.transform.position = new Vector3(x, y, 0);

        grid[x, y] = p;
    }

    public IEnumerator ResolveAfterBooster()
    {
        isResolving = true;

        bool changed;

        // Combo rơi + clear
        do
        {
            while (ApplyFullGravitySmooth())
            {
                yield return new WaitForSeconds(0.25f);
            }

            changed = false;

            yield return StartCoroutine(HandleFullLineCoroutine());
            changed = lastFullLineResult;
            if (changed)
                yield return new WaitForSeconds(0.1f);

        } while (changed);

        // KHÔNG RaiseBoardSmooth()
        // KHÔNG spawn top buffer row
        // KHÔNG tính lượt

        if(GameManager.instance.GetCurrentState() == GameState.Game)
        {
            isResolving = false;
        }
    }

    public bool IsBoardEmpty()
    {
        for (int y = 1; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                if (grid[x, y] != null)
                    return false;
            }
        }
        return true;
    }

    public bool CheckGameOver()
    {
        int dangerRow = 11;

        for (int x = 0; x < width; x++)
        {
            if (grid[x, dangerRow] != null)
                return true;
        }

        return false;
    }

    public void DestroyPiece(Piece piece)
    {
        if (piece == null) return;

        // 1) Xóa khỏi grid NGAY LẬP TỨC (logic)
        RemovePieceFromGrid(piece);

        // 2) Gọi sự kiện cộng điểm ngay lúc logic xoá
        onPieceCleared?.Invoke(piece.score, piece);

        // 3) Chạy hiệu ứng rồi mới Destroy GameObject
        StartCoroutine(DestroyPieceRoutine(piece));
    }

    private IEnumerator DestroyPieceRoutine(Piece piece)
    {
        if (piece == null)
            yield break;

        SpriteRenderer sr = piece.GetComponent<SpriteRenderer>();
        float duration = 0.2f; // thời gian fade

        if (sr != null)
        {
            Color startColor = sr.color;
            float startScale = piece.transform.localScale.x; // giả sử scale đồng đều
            float t = 0f;

            while (t < 1f)
            {
                t += Time.deltaTime / duration;

                // Fade alpha
                Color c = startColor;
                c.a = Mathf.Lerp(1f, 0f, t);
                sr.color = c;

                // Thu nhỏ dần (tuỳ thích, có thể bỏ)
                float s = Mathf.Lerp(startScale, 0.5f * startScale, t);
                piece.transform.localScale = new Vector3(s, s, s);

                yield return null;
            }
        }

        // Đảm bảo object vẫn chưa bị Destroy ở đâu khác
        if (piece != null)
        {
            Destroy(piece.gameObject);
        }
    }

}
