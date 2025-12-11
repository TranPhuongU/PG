using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Image))]
public class Booster : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    Image m_image;
    RectTransform m_rectXform;
    Vector3 m_startPosition;
    Board m_board;
    Tile m_tileTarget;

    public static GameObject ActiveBooster;
    public Text instructionsText;
    public string instructions = "drag over game piece to remove";

    public bool isEnabled = false;
    public bool isDraggable = true;
    public bool isLocked = false;

    public List<CanvasGroup> canvasGroup;
    public UnityEvent boostEvent;
    public int boostTime = 15;

    private void Awake()
    {
        m_image = GetComponent<Image>();

        m_rectXform = GetComponent<RectTransform>();

    }

    private IEnumerator Start()
    {
        while (m_board == null)
        {
            m_board = FindObjectOfType<Board>();
            yield return null; // chờ đến frame kế tiếp
        }

        EnableBooster(false);
    }


    public void EnableBooster(bool state)
    {
        isEnabled = state;

        if (state)
        {
            DisableOtherBoosters();
            Booster.ActiveBooster = gameObject;
        }
        else if (gameObject == Booster.ActiveBooster)
        {
            Booster.ActiveBooster = null;
        }

        m_image.color = (state) ? Color.white : Color.gray;

        if (instructionsText != null)
        {
            instructionsText.gameObject.SetActive(Booster.ActiveBooster != null);

            if (gameObject == Booster.ActiveBooster)
            {
                instructionsText.text = instructions;
            }
        }
    }
    void DisableOtherBoosters()
    {
        Booster[] allBoosters = Object.FindObjectsOfType<Booster>();

        foreach (Booster b in allBoosters)
        {
            if (b != this)
            {
                b.EnableBooster(false);
            }
        }
    }

    public void ToggleBooster()
    {
        EnableBooster(!isEnabled);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked)
        {
            m_startPosition = gameObject.transform.position;
            EnableCanvasGroups(false);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked && Camera.main != null)
        {
            Vector3 onscreenPosition;
            RectTransformUtility.ScreenPointToWorldPointInRectangle(m_rectXform, eventData.position, Camera.main, out onscreenPosition);

            gameObject.transform.position = onscreenPosition;

            RaycastHit2D hit2D = Physics2D.Raycast(onscreenPosition, Vector3.forward, Mathf.Infinity);

            if (hit2D.collider != null)
            {
                m_tileTarget = hit2D.collider.GetComponent<Tile>();
            }
            else
            {
                m_tileTarget = null;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (isEnabled && isDraggable && !isLocked)
        {
            gameObject.transform.position = m_startPosition;

            EnableCanvasGroups(true);

            if (m_board != null && m_board.isResolving)
            {
                return;
            }

            if (m_tileTarget != null)
            {
                if (boostEvent != null)
                {
                    boostEvent.Invoke();
                }

                EnableBooster(false);

                m_tileTarget = null;
                Booster.ActiveBooster = null;
            }
        }
    }

    void EnableCanvasGroups(bool state)
    {
        if (canvasGroup != null && canvasGroup.Count > 0)
        {
            foreach (CanvasGroup cGroup in canvasGroup)
            {
                if (cGroup != null)
                {
                    cGroup.blocksRaycasts = state;
                }
            }
        }
    }

    public void RemoveOneGamePiece()
    {
        if (GameManager.instance.onePieceBoosterAmount <= 0)
            return;

        if (m_board != null && m_tileTarget != null)
        {
            m_board.RemovePieceAt(m_tileTarget.xIndex, m_tileTarget.yIndex);
            GameManager.instance.onePieceBoosterAmount--;
            UIManager.instance.UpdateBoosterTexts();

        }
    }


    public void RemovePieceSameColor()
    {
        if (GameManager.instance.colorPieceBoosterAmount <= 0)
            return;

        if (m_board != null && m_tileTarget != null)
        {
            Piece p = m_board.grid[m_tileTarget.xIndex, m_tileTarget.yIndex];
            if (p != null)
            {
                m_board.RemoveAllSameColor(p);
                GameManager.instance.colorPieceBoosterAmount--;
                UIManager.instance.UpdateBoosterTexts();

            }
        }
    }

    public void ReplacePiece()
    {
        if (GameManager.instance.replacePieceBoosterAmount <= 0)
            return;

        if (m_board != null && m_tileTarget != null)
        {
            Piece p = m_board.grid[m_tileTarget.xIndex, m_tileTarget.yIndex];
            if (p != null && p.booster == PieceBooster.None)
            {
                m_board.ReplacePieceAt(p);
            }
        }
    }
}
