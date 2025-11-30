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
    public float cellSize = 1f;

    public Piece[,] grid;
    public bool isResolving = false;


    void Start()
    {
        grid = new Piece[width, height];
        tiles = new Tile[width, height];

        // tạo tile lưới click
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Vector3 pos = new Vector3(x * cellSize, y * cellSize, 0);
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

    public bool PlacePiece(Piece piece, int x, int ry)
    {
        if (!CanPlace(piece, x, ry))
            return false;

        ClearPiece(piece);

        piece.rootX = x;
        piece.rootY = ry;

        foreach (var c in piece.cells)
            grid[x + c.x, ry + c.y] = piece;

        piece.transform.position = new Vector3(x * cellSize, ry * cellSize, 0);

        return true;
    }

    public void ClearPiece(Piece p)
    {
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                if (grid[x, y] == p)
                    grid[x, y] = null;
    }

    public bool CanMove(Piece p, int dx, int dy)
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

    public bool MovePiece(Piece p, int dx, int dy)
    {
        if (!CanMove(p, dx, dy))
            return false;

        ClearPiece(p);

        p.rootX += dx;
        p.rootY += dy;

        foreach (var c in p.cells)
            grid[p.rootX + c.x, p.rootY + c.y] = p;

        p.MoveSmooth(p.rootX, p.rootY, 0.12f);

        return true;
    }

    public bool CanFall(Piece p)
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


    public bool FallPiece(Piece p) => MovePiece(p, 0, -1);

    public bool ApplyFullGravity()
    {
        bool moved = false;
        bool again = true;

        while (again)
        {
            again = false;

            for (int y = 1; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Piece p = grid[x, y];
                    if (p != null && CanFall(p))
                    {
                        FallPiece(p);
                        moved = true;
                        again = true;
                    }
                }
        }

        return moved;
    }

    public void CheckAndClearLines()
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
            ClearPiece(p);
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
                    while (CanFall(p))
                        FallPiece(p);
            }
        }
    }


    public IEnumerator ResolveAfterMove()
    {
        isResolving = true;

        while (ApplyFullGravity())
            yield return new WaitForSeconds(0.1f);

        CheckAndClearLines();

        while (ApplyFullGravity())
            yield return new WaitForSeconds(1f);

        // 🎯 SAU KHI RƠI + CLEAR XONG HOÀN TOÀN → DÂNG 1 HÀNG
        RaiseAllPieces();

        PieceSpawner.instance.SpawnAreaY0();

        ApplyFullGravity();

        isResolving = false;
    }


    public void OnTileDown(Tile tile)
    {
        if (isResolving) return;

        // lấy piece đang ngồi trên ô này (nếu có)
        draggingPiece = grid[tile.xIndex, tile.yIndex];
        lastTileOver = tile;
    }

    public void OnTileDragOver(Tile tile)
    {
        if (isResolving) return;
        if (draggingPiece == null) return;

        int deltaX = tile.xIndex - lastTileOver.xIndex;

        if (deltaX > 0)
        {
            // kéo sang phải từng ô
            if (MovePiece(draggingPiece, +1, 0))
            {
                lastTileOver = tile;
            }
        }
        else if (deltaX < 0)
        {
            // kéo sang trái từng ô
            if (MovePiece(draggingPiece, -1, 0))
            {
                lastTileOver = tile;
            }
        }
    }

    public void OnTileUp()
    {
        if (isResolving) { draggingPiece = null; return; }
        if (draggingPiece == null) return;

        // sau khi thả → cho hệ thống rơi + clear
        StartCoroutine(ResolveAfterMove());

        draggingPiece = null;
    }
    public void RaiseAllPieces()
    {
        Piece[,] newGrid = new Piece[width, height];

        for (int y = height - 1; y >= 0; y--)
        {
            for (int x = 0; x < width; x++)
            {
                Piece p = grid[x, y];
                if (p == null) continue;

                int newY = y + 1;

                if (newY >= height)
                {
                    // nếu vượt height → bạn quyết định Game Over
                    newY = height - 1;
                }

                newGrid[x, newY] = p;

                p.rootY = newY;
                p.MoveSmooth(p.rootX, p.rootY, 0.12f);
            }
        }

        grid = newGrid;
    }

}
