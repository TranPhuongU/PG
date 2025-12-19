using UnityEngine;

[RequireComponent(typeof(BoxCollider2D))]
public class Tile : MonoBehaviour
{
    public int xIndex;
    public int yIndex;

    Board m_board;

    public void Init(int x, int y, Board board)
    {
        xIndex = x;
        yIndex = y;
        m_board = board;
    }

    void OnMouseDown()
    {
        if (m_board != null)
        {
            m_board.HandlePressTile(this);
        }
    }

    void OnMouseUp()
    {
        if (m_board != null)
        {
            m_board.HandleReleaseTile();
        }
    }
}
