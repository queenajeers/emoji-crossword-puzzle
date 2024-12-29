using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(GridLayoutGroup))]
public class GridCanvas : MonoBehaviour
{
    [SerializeField] private GameObject GridBlockPrefab;
    RectTransform[,] gridBlockRects;

    public float gridBlockBorderSize;
    private GridLayoutGroup gridLayoutGroup;


    float canvasWidth;



    public void Start()
    {
        gridLayoutGroup = GetComponent<GridLayoutGroup>();
        canvasWidth = FindAnyObjectByType<Canvas>().gameObject.GetComponent<RectTransform>().sizeDelta.x;

        CreateBaseGrid(5, 5);
    }


    public void CreateBaseGrid(int rows, int cols)
    {
        StartCoroutine(CreateBaseGridCor(rows, cols));
    }

    IEnumerator CreateBaseGridCor(int rows, int cols)
    {
        gridLayoutGroup.constraintCount = cols;
        gridLayoutGroup.spacing = Vector2.one * gridBlockBorderSize;
        var screenWidth = canvasWidth;
        var cellSize = screenWidth / (float)cols;
        var extraSpace = 4f * (cols - 1);
        cellSize = (screenWidth - ((cellSize * 2f) + extraSpace)) / (float)cols;
        gridLayoutGroup.cellSize = Vector2.one * cellSize;
        int padding = Mathf.CeilToInt(cellSize * 1.2f);

        gridLayoutGroup.padding = new RectOffset(padding, padding, padding, padding);

        Debug.Log($"Screen with is {screenWidth} and cell size is {cellSize}");

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
