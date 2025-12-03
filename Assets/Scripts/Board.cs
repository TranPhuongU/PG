using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
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


    void Start()
    {
        activeFrame = Instantiate(frameSquarePrefab, transform);
        activeFrame.SetActive(false);

        grid = new Piece[width, height];
        tiles = new Tile[width, height];

        // tạo tile lưới click
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x , y , 0);
                GameObject tObj = Instantiate(tilePrefab, pos, Quaternion.identity);
                tObj.transform.SetParent(transform);

                Tile t = tObj.GetComponent<Tile>();
                t.Init(x, y, this);

                tiles[x, y] = t;
            }
        }

        // grid Piece vẫn dùng như code hiện tại
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

        int len = piece.cells.Count;
        float offset = (len % 2 == 0) ? -0.5f : 0f;

        piece.transform.position = new Vector3(
            x + offset,
            ry,
            0
        );

        return true;
    }
    public void RemovePieceFromGrid(Piece p)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == p)
                    grid[x, y] = null;
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
    public void ClearAndCollapseLines()
    {
        for (int y = 1; y < height; y++)   // không check y = 0 !
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                DropAbove(y);
                y--;
            }
        }
    }
    bool IsLineFull(int y)
    {
        for (int x = 0; x < width; x++)
            if (grid[x, y] == null)
                return false;
        return true;
    }
    void ClearLine(int y)
    {
        HashSet<Piece> pieces = new HashSet<Piece>();

        for (int x = 0; x < width; x++)
            if (grid[x, y] != null)
                pieces.Add(grid[x, y]);

        foreach (Piece p in pieces)
        {
            RemovePieceFromGrid(p);
            Destroy(p.gameObject);
        }

        for (int x = 0; x < width; x++)
            grid[x, y] = null;
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

        // ============================
        // PHA 1: GIẢI QUYẾT COMBO TRƯỚC KHI DÂNG
        // ============================
        bool changed;

        do
        {
            // 1. Cho tất cả rơi xuống hết mức có thể
            while (ApplyFullGravitySmooth())
            {
                yield return new WaitForSeconds(.3f);
            }

            // 2. Clear line + DropAbove
            changed = false;

            changed = HandleFullLine(changed);

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
        PieceSpawner.instance.SpawnTopBufferRow();

        do
        {
            // rơi hết sau khi dâng + spawn
            while (ApplyFullGravitySmooth())
            {
                yield return new WaitForSeconds(0.05f);
            }

            changed = false;
            changed = HandleFullLine(changed);

            if (changed)
            {
                yield return new WaitForSeconds(0.1f);
            }

        } while (changed);

        isResolving = false;
    }
    private bool HandleFullLine(bool changed)
    {
        for (int y = 1; y < height; y++)
        {
            if (IsLineFull(y))
            {
                ClearLine(y);
                DropAbove(y);
                y--;
                changed = true;
            }
        }

        return changed;
    }
    public void HandlePressTile(Tile tile)
    {
        if (isResolving) return;

        draggingPiece = grid[tile.xIndex, tile.yIndex];
        lastTileOver = tile;


        if (draggingPiece != null)
        {
            dragStartX = draggingPiece.rootX;
            dragStartY = draggingPiece.rootY;

            ShowPieceFrame(draggingPiece);
            draggingPiece.ActiveFrame(true);
        }

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

        int deltaX = tile.xIndex - lastTileOver.xIndex;

        if (deltaX > 0)
        {
            // kéo sang phải từng ô
            if (MovePieceBy(draggingPiece, +1, 0))
            {
                lastTileOver = tile;
            }
        }
        else if (deltaX < 0)
        {
            // kéo sang trái từng ô
            if (MovePieceBy(draggingPiece, -1, 0))
            {
                lastTileOver = tile;
            }
        }
    }
    public void HandleReleaseTile()
    {
        if (isResolving) { draggingPiece = null; return; }
        if (draggingPiece == null) return;

        // sau khi thả → cho hệ thống rơi + clear
        // Nếu không di chuyển thì KHÔNG tính lượt
        if (draggingPiece.rootX == dragStartX &&
            draggingPiece.rootY == dragStartY)
        {
            // Tắt highlight frame nếu có
            activeFrame?.SetActive(false);
            draggingPiece.ActiveFrame(false);

            draggingPiece = null;
            return; // KHÔNG chạy resolve
        }

        // Nếu có di chuyển → tính lượt
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

        RemovePieceFromGrid(p);
        Destroy(p.gameObject);

        StartCoroutine(ResolveAfterBooster());
    }
    public void RemoveAllSameColor(Piece target)
    {
        if (target == null) return;

        List<Piece> toRemove = new List<Piece>();

        // Tìm tất cả piece cùng màu
        for (int y = 0; y < height; y++)
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

        // Xóa ngay lập tức
        foreach (Piece p in toRemove)
        {
            RemovePieceFromGrid(p);
            Destroy(p.gameObject);
        }

        // Booster KHÔNG tính lượt → dùng coroutine riêng
        StartCoroutine(ResolveAfterBooster());
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

            changed = HandleFullLine(changed);

            if (changed)
                yield return new WaitForSeconds(0.1f);

        } while (changed);

        // KHÔNG RaiseBoardSmooth()
        // KHÔNG spawn top buffer row
        // KHÔNG tính lượt

        isResolving = false;
    }



}
