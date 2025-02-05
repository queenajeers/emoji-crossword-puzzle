
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;
using UnityEngine.EventSystems;


public class ScrollMoveManager : MonoBehaviour, IDragHandler, IBeginDragHandler
{
    public static ScrollMoveManager Instance { get; private set; }
    public RectTransform referenceAreaRect;
    public RectTransform puzzleRect;
    Outline outline;
    private Tween moveTween;

    private Vector2 lastMousePosition;

    float topY;
    float bottomY;

    void Awake()
    {
        Instance = this;
        outline = GetComponent<Outline>();
    }


    public float GetReferenceAreaRectHeight()
    {
        return referenceAreaRect.rect.height;
    }
    public float GetPuzzleAreaRectHeight()
    {
        return puzzleRect.rect.height;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            ArrangePuzzleToTop();
        }
    }

    public void AssignTopBottomYs()
    {
        AssignPuzzleTop();
        AssignPuzzleBottom();
    }

    void AssignPuzzleTop()
    {
        float difference = GetPuzzleAreaRectHeight() - (GetReferenceAreaRectHeight() - 10f);
        float borderSize = Mathf.Abs(outline.effectDistance.y);

        topY = (difference / 2f) + borderSize;

    }
    void AssignPuzzleBottom()
    {
        float difference = GetPuzzleAreaRectHeight() - (GetReferenceAreaRectHeight() - 10f);
        float borderSize = Mathf.Abs(outline.effectDistance.y);
        bottomY = -(difference / 2f) - borderSize;
    }

    void ArrangePuzzleToTop()
    {
        float difference = GetPuzzleAreaRectHeight() - (GetReferenceAreaRectHeight() - 10f);
        float borderSize = Mathf.Abs(outline.effectDistance.y);
        if (difference > 0)
        {
            MoveYBy((-difference / 2f) - borderSize);
        }
    }
    void ArrangePuzzleToBottom()
    {
        float difference = GetPuzzleAreaRectHeight() - (GetReferenceAreaRectHeight() - 10f);
        float borderSize = Mathf.Abs(outline.effectDistance.y);
        if (difference > 0)
        {
            MoveYBy((difference / 2f) + borderSize);
        }
    }

    void MoveYBy(float delta)
    {
        // Kill the previous tween if it exists
        if (moveTween != null && moveTween.IsActive())
        {
            moveTween.Kill();
        }

        // Create a new tween animation
        moveTween = puzzleRect.DOAnchorPosY(delta, 0.3f)
            .SetEase(Ease.OutQuad); // Adjust duration and easing as needed
    }

    public void MovePuzzleBoardRelativeTo(PuzzleBlock puzzleBlock)
    {
        float refHeight = GetReferenceAreaRectHeight();
        // float topPointY = (referenceAreaRect.anchoredPosition + (Vector2.up * (refHeight / 2f))).y;
        // float bottomPointY = (referenceAreaRect.anchoredPosition + (Vector2.down * (refHeight / 2f))).y;

        float topPointY = refHeight / 2f;
        float bottomPointY = -1 * refHeight / 2f;

        Debug.Log("TOP Y IS :" + topPointY);
        Debug.Log("BOTTOM Y IS :" + bottomPointY);

        var puzzleBlockPos = puzzleBlock.GetPosition(referenceAreaRect);
        float heighOfPuzzleBlock = puzzleBlock.GetHeightOfRect();
        float borderSizeOfPuzzleBlock = puzzleBlock.GetBorderSize();

        // IS BOTTOM
        //puzzleBlockPos.y < bottomPointY
        if (IsBoxOverlappingLine(puzzleBlockPos.y, heighOfPuzzleBlock, bottomPointY) || IsBoxBelowLine(puzzleBlockPos.y, heighOfPuzzleBlock, bottomPointY))
        {
            // float difference = Math.Abs(bottomPointY - puzzleBlockPos.y);
            // MoveYBy(difference + (heighOfPuzzleBlock / 2f) + borderSizeOfPuzzleBlock);
            ArrangePuzzleToBottom();
        }
        // IS TOP
        //puzzleBlockPos.y > topPointY
        else if (IsBoxOverlappingLine(puzzleBlockPos.y, heighOfPuzzleBlock, topPointY) || IsBoxAboveLine(puzzleBlockPos.y, heighOfPuzzleBlock, topPointY))
        {
            // float difference = puzzleBlockPos.y - topPointY;
            // MoveYBy(-(difference + (heighOfPuzzleBlock / 2f) + borderSizeOfPuzzleBlock));
            ArrangePuzzleToTop();
        }
    }

    public bool IsBoxOverlappingLine(float boxY, float boxHeight, float lineY)
    {
        float boxTop = boxY + (boxHeight / 2);
        float boxBottom = boxY - (boxHeight / 2);

        return lineY >= boxBottom && lineY <= boxTop;
    }
    public bool IsBoxBelowLine(float boxY, float boxHeight, float lineY)
    {
        float boxTop = boxY + (boxHeight / 2);
        return boxTop < lineY;
    }

    public bool IsBoxAboveLine(float boxY, float boxHeight, float lineY)
    {
        float boxBottom = boxY - (boxHeight / 2);
        return boxBottom > lineY;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastMousePosition;
        delta.x = 0;
        var pos = puzzleRect.anchoredPosition + delta;
        pos.y = Mathf.Clamp(pos.y, bottomY, topY);
        puzzleRect.anchoredPosition = pos; // Move panel
        lastMousePosition = eventData.position;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        lastMousePosition = eventData.position;
    }
}
