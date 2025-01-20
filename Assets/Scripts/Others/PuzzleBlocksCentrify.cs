using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class PuzzleBlocksCentrify : MonoBehaviour
{
    public static PuzzleBlocksCentrify Instance;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public List<PuzzleBlock> childPuzzleBlocks = new List<PuzzleBlock>();
    public List<RectTransform> puzzleBlockRects = new List<RectTransform>();
    public RectTransform relativeToPanel;
    public RectTransform parentPanel;
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            CenterRects();
        }
    }

    public void LoadChildPuzzleBlocks()
    {
        childPuzzleBlocks = GetComponentsInChildren<PuzzleBlock>().ToList();
        for (int i = 0; i < childPuzzleBlocks.Count; i++)
        {
            puzzleBlockRects.Add(childPuzzleBlocks[i].rectTransform);
        }
    }

    public void CenterRects()
    {
        if (puzzleBlockRects == null || puzzleBlockRects.Count == 0) return;

        Vector2 panelCenter = relativeToPanel.rect.center; // Panel center relative to itself

        // Step 1: Calculate the horizontal bounding box of all rects
        float minX = float.MaxValue, maxX = float.MinValue;

        for (int i = 0; i < puzzleBlockRects.Count; i++)
        {
            Vector2 pos = puzzleBlockRects[i].localPosition;
            minX = Mathf.Min(minX, pos.x);
            maxX = Mathf.Max(maxX, pos.x);
        }

        // Step 2: Find the horizontal center of the bounding box
        float boundingBoxCenterX = (minX + maxX) / 2;

        // Step 3: Calculate the horizontal offset to align with the panel center
        float offsetX = panelCenter.x - boundingBoxCenterX;

        // Step 4: Apply the horizontal offset while preserving Y positions
        for (int i = 0; i < childPuzzleBlocks.Count; i++)
        {
            Vector2 originalPos = puzzleBlockRects[i].localPosition;
            childPuzzleBlocks[i].GoToPosition(new Vector2(originalPos.x + offsetX, originalPos.y));
        }
        PuzzleBlockSelector.Instance.GoToPosition();
    }

    public void RemovePuzzleBlock(PuzzleBlock puzzleBlock, bool animated)
    {
        if (!animated)
        {
            int indexOf = childPuzzleBlocks.IndexOf(puzzleBlock);
            if (indexOf != -1)
            {
                childPuzzleBlocks.RemoveAt(indexOf);
                puzzleBlockRects.RemoveAt(indexOf);
            }
            puzzleBlock.gameObject.SetActive(false);
            CenterRects();
        }
        else
        {
            PuzzleBlockSelector.Instance.avoidTouch = true;
            puzzleBlock.transform.DOScale(0f, .2f).OnComplete(() =>
            {
                int indexOf = childPuzzleBlocks.IndexOf(puzzleBlock);
                if (indexOf != -1)
                {
                    puzzleBlock.gameObject.SetActive(false);
                    PuzzleBlockSelector.Instance.avoidTouch = false;
                    childPuzzleBlocks.RemoveAt(indexOf);
                    puzzleBlockRects.RemoveAt(indexOf);
                }
                CenterRects();
            });
        }
    }





}
