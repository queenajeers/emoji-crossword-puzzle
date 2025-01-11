using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public delegate void RemoveFromPlacedLocationEvent(Vector2Int location);
public class PuzzleLoader : MonoBehaviour
{
    public static PuzzleLoader Instance { get; private set; }
    public string puzzleName;
    public PuzzleDifficulty puzzleDifficulty;
    private EmojiCrossWord emojiCrossWord = new EmojiCrossWord();
    public GameObject puzzleBlockPrefab;
    public Transform puzzleParent;

    public float gridSizeMultiplier;
    public float borderSize;
    public Color borderColor;

    public Transform draggableLettersParent;
    Dictionary<Vector2Int, char> correctCharacters = new Dictionary<Vector2Int, char>();
    Dictionary<Vector2Int, Vector2> draggablesSnapPositions = new Dictionary<Vector2Int, Vector2>();
    Dictionary<Vector2Int, PuzzleBlock> emptyPuzzleBlocks = new Dictionary<Vector2Int, PuzzleBlock>();
    Dictionary<Vector2Int, PuzzleBlock> allPuzzleBlocks = new Dictionary<Vector2Int, PuzzleBlock>();

    Dictionary<string, List<PuzzleBlock>> wordLinkedPuzzleBlocks = new Dictionary<string, List<PuzzleBlock>>();
    List<string> finishedWords = new List<string>();
    DraggableLetter currentDraggingLetter;
    private PuzzleBlock currentNearestPuzzleBlock;
    public float thresholdForNearestPuzzleBlock;

    public int maxDraggableLetters;
    int currentPlacedCorrectly = 0;
    List<Vector2Int> placedLocations = new List<Vector2Int>();
    private List<char> leftOverLetters = new List<char>(); // bug: O LETTER NOT SPAWNED

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        //emojiCrossWord = emojiCrossWord.LoadFromSavedPath(GetSavePath());
        emojiCrossWord = emojiCrossWord.LoadFromSavedPathMobile(puzzleName, puzzleDifficulty);
        if (emojiCrossWord != null)
        {
            StartCoroutine(LoadPuzzle());
        }
        else
        {
            Debug.LogWarning($"No puzzle with name=> {puzzleName} in difficulty level=> {puzzleDifficulty} found!");
        }

        DraggableLetter.OnLetterDraggingStarted += OnLetterDragableDragStarted;
        DraggableLetter.OnLetterDraggingEnded += OnLetterDragableDragEnded;
        DraggableLetter.OnLetterReturned += OnLetterDraggableRetuned;
    }

    void Update()
    {
        if (currentDraggingLetter != null)
        {
            HighlightNearestPuzzleBlock();
        }
        if (Input.GetKeyDown(KeyCode.F))
        {
            LevelFinished();
        }

    }

    public void BringNextSetOfLetters()
    {
        DraggableLettersContainer.Instance.LoadLetters(GiveNextSetOfLetters());

    }

    IEnumerator LoadPuzzle()
    {

        if (GridLayer.Instance != null)
        {
            Vector2Int gridSize = emojiCrossWord.gridSize;
            GridLayer.Instance.SetCellBorderSize(borderSize);
            GridLayer.Instance.CreateBaseGrid(gridSize.x, gridSize.y);
            GridLayer.Instance.gridSizeMultiplier = gridSizeMultiplier;
            DraggableLettersContainer.Instance.InitialiseSlots(maxDraggableLetters);

            yield return new WaitForSeconds(.4f);

            GetComponent<Outline>().effectDistance = new Vector2(borderSize * 2f, -borderSize * 2f);
            GetComponent<Outline>().effectColor = borderColor;

            GetComponent<RectTransform>().sizeDelta = GridLayer.Instance.GetComponent<RectTransform>().sizeDelta;
            GetComponent<RectTransform>().offsetMax = GridLayer.Instance.GetComponent<RectTransform>().offsetMax;
            GetComponent<RectTransform>().offsetMin = GridLayer.Instance.GetComponent<RectTransform>().offsetMin;

            foreach (var dataBlock in emojiCrossWord.dataBlocks)
            {
                Vector2Int blockLocation = dataBlock.blockLocation;

                var puzzleBlock = Instantiate(puzzleBlockPrefab, puzzleParent);

                var puzzleBlockRect = puzzleBlock.GetComponent<RectTransform>();
                puzzleBlockRect.localPosition = GridLayer.Instance.GetPositionOfAGridBlock(blockLocation.x, blockLocation.y, transform);
                puzzleBlockRect.sizeDelta = Vector2.one * GridLayer.Instance.GetCellSize();

                var puzzleBlockComp = puzzleBlock.GetComponent<PuzzleBlock>();
                puzzleBlockComp.SetOutline(borderColor, borderSize);
                puzzleBlockComp.SetHintArrowIndication(dataBlock.arrowIndications);
                puzzleBlockComp.blockLocation = blockLocation;
                allPuzzleBlocks[blockLocation] = puzzleBlockComp;
                if (dataBlock is LetterBox letterBox)
                {
                    puzzleBlockComp.correctLetter = letterBox.letter;
                    correctCharacters[dataBlock.blockLocation] = letterBox.letter;
                    if (!letterBox.isClue)
                    {
                        // Not Empty Boxs
                        puzzleBlockComp.LoadAsNormalText(letterBox.letter);
                    }
                    else
                    {
                        // Empty Box
                        leftOverLetters.Add(letterBox.letter);
                        draggablesSnapPositions[blockLocation] = GridLayer.Instance.GetPositionOfAGridBlock(blockLocation.x, blockLocation.y, draggableLettersParent);
                        emptyPuzzleBlocks[blockLocation] = puzzleBlockComp;
                    }
                }
                else if (dataBlock is Hint hintBox)
                {
                    Sprite sprite = Resources.Load<Sprite>(hintBox.localPath);
                    puzzleBlockComp.LoadAsHintBlock(sprite);
                }

            }

            int currentWordIndex = 0;
            for (int col = 0; col < emojiCrossWord.gridSize.y; col++)
            {
                for (int row = 0; row < emojiCrossWord.gridSize.x; row++)
                {
                    Vector2Int blockLocation = new Vector2Int(row, col);
                    DataBlock dataBlock = emojiCrossWord.dataBlocks.Find(db => db.blockLocation == blockLocation);
                    if (dataBlock != null)
                    {
                        if (dataBlock.arrowIndications.Count > 0)
                        {
                            allPuzzleBlocks[blockLocation].SetWordIndex(currentWordIndex);
                            currentWordIndex++;
                        }
                    }
                }
            }
            foreach (var word in emojiCrossWord.crossWords)
            {
                wordLinkedPuzzleBlocks[word] = GetPuzzleBlocksForWord(word);
            }

            UIUtils.Shuffle(leftOverLetters);
            BringNextSetOfLetters();

            draggableLettersParent.SetAsLastSibling();
            yield return null;
        }
    }

    public string GetSavePath()
    {
#if UNITY_ANDROID || UNITY_IOS
        // Mobile-specific logic using Resources.Load
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

        string relativePath = Path.Combine("Levels", difficultyFolder, puzzleName).Replace("\\", "/");
        TextAsset saveFile = Resources.Load<TextAsset>(relativePath);

        if (saveFile == null)
        {
            Debug.LogError($"Save file not found at Resources/{relativePath}");
        }

        return saveFile != null ? $"Resources/{relativePath}" : null; // Return a string for consistency
#else
    // Non-mobile platforms logic using Application.dataPath
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
#endif
    }

    public void HighlightNearestPuzzleBlock()
    {
        float minDist = Mathf.Infinity;
        Vector2Int targetGridLocation = new Vector2Int(-1, -1);
        foreach (var blockLocation in draggablesSnapPositions.Keys)
        {
            if (placedLocations.Contains(blockLocation)) continue;
            float dist = Vector2.Distance(draggablesSnapPositions[blockLocation], currentDraggingLetter.CurrentLocalPosition());
            if (dist < minDist && dist < thresholdForNearestPuzzleBlock)
            {
                minDist = dist;
                targetGridLocation = blockLocation;
            }
        }

        if (currentNearestPuzzleBlock != null)
        {
            currentNearestPuzzleBlock.NormaliseBG();
            currentNearestPuzzleBlock = null;
        }

        if (emptyPuzzleBlocks.ContainsKey(targetGridLocation))
        {
            currentNearestPuzzleBlock = emptyPuzzleBlocks[targetGridLocation];

            currentNearestPuzzleBlock.HighlightBG();
            currentDraggingLetter.SetToSelectedPosition(draggablesSnapPositions[targetGridLocation]);
        }

    }

    private void OnLetterDragableDragStarted(DraggableLetter draggableLetter)
    {
        currentDraggingLetter = draggableLetter;
    }
    private void OnLetterDragableDragEnded(DraggableLetter draggableLetter)
    {
        currentDraggingLetter = null;
        if (currentNearestPuzzleBlock != null)
        {
            draggableLetter.placedAtLocation = currentNearestPuzzleBlock.blockLocation;
            placedLocations.Add(currentNearestPuzzleBlock.blockLocation);
            currentNearestPuzzleBlock.NormaliseBG();
            currentNearestPuzzleBlock = null;
        }
        else
        {
            draggableLetter.placedAtLocation = new Vector2Int(-1, -1);
            draggableLetter.SetReturnPositionOriginalPosition();
        }
    }

    public PuzzleBlock GetPuzzleBlockAtLocation(Vector2Int location)
    {
        return allPuzzleBlocks[location];
    }

    bool HighlightFinishedWord(char lastPlacedLetter)
    {
        List<string> finished = new List<string>();
        foreach (var word in wordLinkedPuzzleBlocks.Keys)
        {
            if (word.Contains(lastPlacedLetter) && !finishedWords.Contains(word))
            {
                var emptyBlock = wordLinkedPuzzleBlocks[word].Find(pb => pb.filledCorrectly == false && !pb.isHint);
                if (emptyBlock == null)
                {
                    MarkWordAsFinished(wordLinkedPuzzleBlocks[word]);
                    finished.Add(word);
                    finishedWords.Add(word);
                }
            }
        }

        return finished.Count > 0;
    }

    public void MarkWordAsFinished(List<PuzzleBlock> puzzleBlocks)
    {
        for (int i = 0; i < puzzleBlocks.Count; i++)
        {
            puzzleBlocks[i].MarkAsCorrectBlock(i);
        }

    }

    private void OnLetterDraggableRetuned(DraggableLetter draggableLetter)
    {
        if (draggableLetter.letter == correctCharacters[draggableLetter.placedAtLocation])
        {
            // CORRECT
            draggableLetter.placedCorrectly = true;

            emptyPuzzleBlocks[draggableLetter.placedAtLocation].LoadAsNormalText(draggableLetter.letter);
            currentPlacedCorrectly++;
            if (currentPlacedCorrectly == maxDraggableLetters)
            {
                currentPlacedCorrectly = 0;
                BringNextSetOfLetters();
            }
            draggableLetter.ActivateCorrectPlacementEffects(HighlightFinishedWord(draggableLetter.letter));
            if (finishedWords.Count == wordLinkedPuzzleBlocks.Count)
            {
                LevelFinished();
            }
        }
        else
        {
            // WRONG
            draggableLetter.avoidTouch = true;
            draggableLetter.ActivateWrongPlacementEffects();
            draggableLetter.GoBackToOriginalPosition(RemoveFromPlacedLocations);
        }
    }

    public void RemoveFromPlacedLocations(Vector2Int location)
    {
        placedLocations.Remove(location);
    }


    public char[] GiveNextSetOfLetters()
    {
        return leftOverLetters
            .Skip(placedLocations.Count)  // Skip the already placed letters
            .Take(maxDraggableLetters)     // Take the required number of letters
            .ToArray();                    // Convert the result to a char array
    }

    public List<PuzzleBlock> GetPuzzleBlocksForWord(string word)
    {
        string capitalisedWord = word.ToUpper();
        char firstLetter = word[0];
        List<LetterBox> possibleTargetLetters = emojiCrossWord.dataBlocks.OfType<LetterBox>().Where(db => db.letter == firstLetter).ToList();
        foreach (LetterBox targetLetter in possibleTargetLetters)
        {
            foreach (ArrowIndication arrowIndication in targetLetter.arrowIndications)
            {
                var targetPuzzleBlocks = GetTargetBlocks(targetLetter.blockLocation, word, arrowIndication);
                if (targetPuzzleBlocks.Count > 0)
                {
                    return targetPuzzleBlocks;
                }
            }
        }
        return null;
    }

    public List<PuzzleBlock> GetTargetBlocks(Vector2Int startLocation, string word, ArrowIndication directionToSearch)
    {
        List<PuzzleBlock> targetPuzzleBlocks = new();

        if (directionToSearch != ArrowIndication.None)
        {
            _ = new Vector2Int(-1, -1);
            Vector2Int hintLocation;
            switch (directionToSearch)
            {
                case ArrowIndication.FromLeft:

                    hintLocation = startLocation - new Vector2Int(0, 1);
                    if (allPuzzleBlocks.ContainsKey(hintLocation))
                    {
                        targetPuzzleBlocks.Add(allPuzzleBlocks[hintLocation]);
                    }
                    break;
                case ArrowIndication.FromRight:
                    hintLocation = startLocation + new Vector2Int(0, 1);
                    if (allPuzzleBlocks.ContainsKey(hintLocation))
                    {
                        targetPuzzleBlocks.Add(allPuzzleBlocks[hintLocation]);
                    }
                    break;
                case ArrowIndication.FromTop:
                    hintLocation = startLocation - new Vector2Int(1, 0);
                    if (allPuzzleBlocks.ContainsKey(hintLocation))
                    {
                        targetPuzzleBlocks.Add(allPuzzleBlocks[hintLocation]);
                    }
                    break;
                case ArrowIndication.FromBottom:
                    hintLocation = startLocation + new Vector2Int(1, 0);
                    if (allPuzzleBlocks.ContainsKey(hintLocation))
                    {
                        targetPuzzleBlocks.Add(allPuzzleBlocks[hintLocation]);
                    }
                    break;
            }
            for (int x = 0; x < word.Length; x++)
            {
                var searchLocation = startLocation;
                switch (directionToSearch)
                {
                    case ArrowIndication.FromLeft:

                        searchLocation = startLocation + new Vector2Int(0, x);
                        break;
                    case ArrowIndication.FromRight:
                        searchLocation = startLocation - new Vector2Int(0, x);
                        break;
                    case ArrowIndication.FromTop:
                        searchLocation = startLocation + new Vector2Int(x, 0);
                        break;
                    case ArrowIndication.FromBottom:
                        searchLocation = startLocation - new Vector2Int(x, 0);
                        break;
                }

                if (!allPuzzleBlocks.ContainsKey(searchLocation))
                {
                    targetPuzzleBlocks.Clear();
                    break;
                }

                if (word[x] == allPuzzleBlocks[searchLocation].correctLetter)
                {
                    targetPuzzleBlocks.Add(allPuzzleBlocks[searchLocation]);
                }

                else
                {
                    targetPuzzleBlocks.Clear();
                    break;
                }
            }
        }

        return targetPuzzleBlocks;
    }

    public void LevelFinished()
    {
        StartCoroutine(LevelFinishedCor());
    }

    IEnumerator LevelFinishedCor()
    {
        yield return new WaitForSeconds(.3f);

        puzzleParent.DOScale(1.05f, .2f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            puzzleParent.DOScale(1f, .9f).SetEase(Ease.OutElastic);
        });

        yield return new WaitForSeconds(.2f + .9f);
        foreach (var blocks in wordLinkedPuzzleBlocks.Values)
        {
            for (int i = 0; i < blocks.Count; i++)
            {
                blocks[i].FadeOut(i);
            }
            yield return new WaitForSeconds(.2f);
        }
    }

}
