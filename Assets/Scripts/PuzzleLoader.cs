using System.Collections;
using System.IO;
using UnityEngine;

public class PuzzleLoader : MonoBehaviour
{
    public string puzzleName;
    public PuzzleDifficulty puzzleDifficulty;
    private EmojiCrossWord emojiCrossWord = new EmojiCrossWord();
    public GameObject puzzleBlockPrefab;
    public Transform puzzleParent;

    public float borderSize;
    public Color borderColor;

    private void Start()
    {
        emojiCrossWord = emojiCrossWord.LoadFromSavedPath(GetSavePath());

        if (emojiCrossWord != null)
        {
            StartCoroutine(LoadPuzzle());
        }
        else
        {
            Debug.LogWarning($"No puzzle with name=> {puzzleName} in difficulty level=> {puzzleDifficulty} found!");
        }
    }

    IEnumerator LoadPuzzle()
    {
        if (GridLayer.Instance != null)
        {
            Vector2Int gridSize = emojiCrossWord.gridSize;
            GridLayer.Instance.SetCellBorderSize(borderSize);
            GridLayer.Instance.CreateBaseGrid(gridSize.x, gridSize.y);

            yield return new WaitForSeconds(.4f);

            GetComponent<RectTransform>().sizeDelta = GridLayer.Instance.GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().offsetMax = GridLayer.Instance.GetComponent<RectTransform>().offsetMax;
            GetComponent<RectTransform>().offsetMin = GridLayer.Instance.GetComponent<RectTransform>().offsetMin;

            foreach (var dataBlock in emojiCrossWord.dataBlocks)
            {
                var blockLocation = dataBlock.blockLocation;
                var puzzleBlock = Instantiate(puzzleBlockPrefab, puzzleParent);

                var puzzleBlockRect = puzzleBlock.GetComponent<RectTransform>();
                puzzleBlockRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(blockLocation.x, blockLocation.y, transform);
                puzzleBlockRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();

                var puzzleBlockComp = puzzleBlock.GetComponent<PuzzleBlock>();
                puzzleBlockComp.SetOutline(borderColor, borderSize);
                puzzleBlockComp.SetHintArrowIndication(dataBlock.arrowIndications);

                if (dataBlock is LetterBox letterBox)
                {
                    if (!letterBox.isClue)
                    {
                        puzzleBlockComp.LoadAsNormalText(letterBox.letter);
                    }
                }
                else if (dataBlock is Hint hintBox)
                {
                    Sprite sprite = Resources.Load<Sprite>(hintBox.localPath);
                    puzzleBlockComp.LoadAsHintBlock(sprite);
                }

            }

            yield return null;
        }
    }

    public string GetSavePath()
    {
        string difficultyFolder = "";
        switch (puzzleDifficulty)
        {
            case PuzzleDifficulty.Easy:
                difficultyFolder = "Easy";
                break;
            case PuzzleDifficulty.Hard:
                difficultyFolder = "Hard";
                break;
            case PuzzleDifficulty.Expert:
                difficultyFolder = "Expert";
                break;
        }
        string savePath = Path.Combine(Application.dataPath, "Resources", "Levels", difficultyFolder, $"{puzzleName}.json");
        return savePath;
    }

}
