using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridLayer : MonoBehaviour
{
    public static GridLayer Instance { get; private set; }
    [SerializeField] private GameObject GridBlockPrefab;
    RectTransform[,] gridBlockRects;

    public float gridBlockBorderSize;
    public float gridSizeMultiplier = 1f;

    private GridLayoutGroup gridLayoutGroup;

    float canvasWidth;
    float cellSize;

    public void Awake()
    {
        Instance = this;
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
    }

    public void CreateBaseGrid(int rows, int cols)
    {
        StartCoroutine(CreateBaseGridCor(rows, cols));
    }

    public float GetCellSize()
    {
        return cellSize;
    }

    public float GetCellBorderSize()
    {
        return gridBlockBorderSize;
    }
    public void SetCellBorderSize(float borderSize)
    {
        gridBlockBorderSize = borderSize;
    }


    IEnumerator CreateBaseGridCor(int rows, int cols)
    {
        yield return null;
        canvasWidth = FindFirstObjectByType<Canvas>().gameObject.GetComponent<RectTransform>().sizeDelta.x;

        float extraSpace = 40;
        gridLayoutGroup.constraintCount = cols;
        gridLayoutGroup.spacing = Vector2.one * gridBlockBorderSize;

        // Total spacing, borders, and extra space to account for in the width
        float totalSpacingAndBorders = (cols - 1) * gridBlockBorderSize + 2 * gridBlockBorderSize + 2 * extraSpace;

        // Calculate the cell size
        cellSize = (canvasWidth - totalSpacingAndBorders) / cols;
        cellSize *= gridSizeMultiplier;
        gridLayoutGroup.cellSize = Vector2.one * cellSize;

        // Adjust padding to include extra space
        int paddingX = Mathf.CeilToInt(20);
        int paddingY = Mathf.CeilToInt(20);

        //gridLayoutGroup.padding = new RectOffset(paddingX, paddingX, paddingY, paddingY);

        Debug.Log($"Screen width is {canvasWidth}, cell size is {cellSize}, total spacing+borders+extraSpace is {totalSpacingAndBorders}");

        gridBlockRects = new RectTransform[rows, cols];
        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < cols; col++)
            {
                var gridBlock = Instantiate(GridBlockPrefab, transform);
                gridBlock.GetComponent<Outline>().effectDistance = new Vector2(gridBlockBorderSize, -gridBlockBorderSize);
                gridBlockRects[row, col] = gridBlock.GetComponent<RectTransform>();
            }
        }

        yield return null;
    }


    public Vector2 GetPositionOfAGridBlock(int row, int col, Transform relativeTransform)
    {
        return UIUtils.GetCenterAnchorUIPos(gridBlockRects[row, col], relativeTransform);
    }


}
