using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public enum PuzzleCreatorBrush
{
    None,

}

public class PuzzleCreator : MonoBehaviour
{
    public GameObject puzzleCreateTouchBlock;

    public EmojiCrossWord emojiCrossWord;
    public string levelName;

    private void Start()
    {
        StartCoroutine(CreateTouchBase());
    }

    IEnumerator CreateTouchBase()
    {
        if (GridLayer.Instance != null)
        {
            var gridSize = emojiCrossWord.gridSize;

            GridLayer.Instance.CreateBaseGrid(gridSize.x, gridSize.y);
            yield return new WaitForSeconds(.1f);

            GetComponent<RectTransform>().sizeDelta = GridLayer.Instance.GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().offsetMax = GridLayer.Instance.GetComponent<RectTransform>().offsetMax;

            for (int row = 0; row < gridSize.x; row++)
            {
                for (int col = 0; col < gridSize.y; col++)
                {
                    var gridTouchBlock = Instantiate(puzzleCreateTouchBlock, transform);
                    var gridTouchRect = gridTouchBlock.GetComponent<RectTransform>();
                    var cellBorderSize = GridLayer.Instance.GetCellBorderSize();
                    gridTouchBlock.GetComponent<Outline>().effectDistance = new Vector2(cellBorderSize, -cellBorderSize);
                    gridTouchRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(row, col, transform);
                    gridTouchRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();
                }
            }

        }

    }

    public void SavePuzzle()
    {
        Debug.Log("SAVE PUZZLE");
    }

}
