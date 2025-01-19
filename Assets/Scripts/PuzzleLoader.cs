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
    EmojiCrossWord emojiCrossWord = new EmojiCrossWord();
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
    Dictionary<Vector2Int, LetterBox> letterBoxes = new Dictionary<Vector2Int, LetterBox>();
    List<string> finishedWords = new List<string>();
    DraggableLetter currentDraggingLetter;
    private PuzzleBlock currentNearestPuzzleBlock;
    public float thresholdForNearestPuzzleBlock;

    public int maxDraggableLetters;
    List<Vector2Int> placedLocations = new List<Vector2Int>();
    private List<char> leftOverLetters = new List<char>();
    [SerializeField] string gameFilesFolderName;
    List<string> unsolvedCrossWords = new List<string>();

    void Awake()
    {
        Instance = this;
    }
    private void Start()
    {
        FirstTimeInitialisations();

        DraggableLetter.OnLetterDraggingStarted += OnLetterDragableDragStarted;
        DraggableLetter.OnLetterDraggingEnded += OnLetterDragableDragEnded;
        DraggableLetter.OnLetterReturned += OnLetterDraggableRetuned;
    }

    void OnDestroy()
    {
        DraggableLetter.OnLetterDraggingStarted -= OnLetterDragableDragStarted;
        DraggableLetter.OnLetterDraggingEnded -= OnLetterDragableDragEnded;
        DraggableLetter.OnLetterReturned -= OnLetterDraggableRetuned;
    }

    void FirstTimeInitialisations()
    {
        if (PlayerPrefs.GetInt($"FirstTime_{puzzleName}", 0) == 0)
        {
            if (Directory.Exists(GetPersistantSaveFilePath()))
            {
                Directory.Delete(GetPersistantSaveFilePath());
            }
            PlayerPrefs.SetInt($"FirstTime_{puzzleName}", 1);
            emojiCrossWord = emojiCrossWord.LoadFromSavedPathMobile(puzzleName, puzzleDifficulty);
            CreateSaveDirectory(GetPersistantSaveFolderPath());
            if (emojiCrossWord != null)
            {
                emojiCrossWord.SaveCrossWordPuzzle(GetPersistantSaveFilePath());
            }
        }
        else
        {
            emojiCrossWord = emojiCrossWord.LoadFromSavedPath(GetPersistantSaveFilePath());
        }

        if (emojiCrossWord != null)
        {
            StartCoroutine(LoadPuzzle());
        }
        else
        {
            Debug.LogWarning($"No puzzle with name=> {puzzleName} in difficulty level=> {puzzleDifficulty} found!");
        }
    }

    public void SavePuzzle()
    {
        emojiCrossWord.SaveCrossWordPuzzle(GetPersistantSaveFilePath());
    }

    public string GetPersistantSaveFolderPath()
    {
        return Path.Combine(Application.persistentDataPath, gameFilesFolderName, puzzleDifficulty.ToString());
    }
    public string GetPersistantSaveFilePath()
    {
        return Path.Combine(Application.persistentDataPath, gameFilesFolderName, puzzleDifficulty.ToString(), puzzleName);
    }

    public void CreateSaveDirectory(string path)
    {

        if (!Directory.Exists(path))
        {
            // Create the folder
            Directory.CreateDirectory(path);
            Debug.Log($"Folder created at: {path}");
        }
        else
        {
            Debug.Log($"Folder already exists at: {path}");
        }
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
        if (emojiCrossWord.currentLeftOverWords.Count == 0)
        {
            emojiCrossWord.currentLeftOverWords = new List<char>(GiveNextSetOfLetters());
        }

        //DraggableLettersContainer.Instance.LoadLetters(emojiCrossWord.currentLeftOverWords.ToArray());

    }

    IEnumerator LoadPuzzle()
    {

        if (GridLayer.Instance != null)
        {
            Vector2Int gridSize = emojiCrossWord.gridSize;
            GridLayer.Instance.SetCellBorderSize(borderSize);
            GridLayer.Instance.CreateBaseGrid(gridSize.x, gridSize.y);
            GridLayer.Instance.gridSizeMultiplier = gridSizeMultiplier;

            //DraggableLettersContainer.Instance?.InitialiseSlots(maxDraggableLetters);

            yield return new WaitForSeconds(.4f);


            PuzzleBlockSelector.Instance.SetSelectorDimensions(GridLayer.Instance.GetCellSize(), borderColor, borderSize);
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
                    letterBoxes[blockLocation] = letterBox;
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

            List<PuzzleBlock> numberedBlocks = new List<PuzzleBlock>();

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
                            numberedBlocks.Add(allPuzzleBlocks[blockLocation]);
                            currentWordIndex++;
                        }
                    }
                }
            }
            foreach (var word in emojiCrossWord.crossWords)
            {
                wordLinkedPuzzleBlocks[word] = GetPuzzleBlocksForWord(word);
            }

            for (int i = 0; i < numberedBlocks.Count; i++)
            {
                var partOfWords = numberedBlocks[i].partOfWords;
                for (int j = 0; j < partOfWords.Count; j++)
                {
                    unsolvedCrossWords.Add(partOfWords[j]);
                }
            }
            unsolvedCrossWords = unsolvedCrossWords
      .Where(new HashSet<string>().Add)
      .ToList();

            PuzzleBlocksCentrify.Instance.LoadChildPuzzleBlocks();

            HighlightFinishedWordsNoAnimation();
            PuzzleBlockSelector.Instance.selectorRect.transform.SetAsLastSibling();

            if (emojiCrossWord.totalLeftOverWords.Count == 0)
            {
                UIUtils.Shuffle(leftOverLetters);
                emojiCrossWord.totalLeftOverWords = new List<char>(leftOverLetters);
                SavePuzzle();
            }
            else
            {
                leftOverLetters = emojiCrossWord.totalLeftOverWords;
            }

            BringNextSetOfLetters();

            draggableLettersParent.SetAsLastSibling();


            foreach (var item in wordLinkedPuzzleBlocks.Values)
            {
                item[0].gameObject.SetActive(false);
            }

            HighlightNextWord();

            PuzzleBlocksCentrify.Instance.CenterRects();

            yield return null;
        }
    }

    public void SetWordAsFinished(string word)
    {
        unsolvedCrossWords.Remove(word);
        foreach (var pb in wordLinkedPuzzleBlocks[word])
        {
            pb.partOfWords.Remove(word);
        }
        MarkWordAsFinished(word);
    }
    public void HighlightNextWord()
    {
        if (unsolvedCrossWords.Count > 0)
        {
            Debug.LogWarning($"Highlighting word {unsolvedCrossWords[0]}");
            wordLinkedPuzzleBlocks[unsolvedCrossWords[0]][1].SelectThis();
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
    void HighlightFinishedWordsNoAnimation()
    {
        List<string> finished = new List<string>();
        foreach (var word in wordLinkedPuzzleBlocks.Keys)
        {
            if (!finishedWords.Contains(word))
            {
                var emptyBlock = wordLinkedPuzzleBlocks[word].Find(pb => pb.filledCorrectly == false && !pb.isHint);
                if (emptyBlock == null)
                {
                    MarkWordAsFinished(wordLinkedPuzzleBlocks[word], true);

                    finished.Add(word);
                    finishedWords.Add(word);
                    unsolvedCrossWords.Remove(word);
                    foreach (var item in wordLinkedPuzzleBlocks[word])
                    {
                        item.partOfWords.Remove(word);
                    }
                }
            }
        }

    }

    public void MarkWordAsFinished(List<PuzzleBlock> puzzleBlocks, bool noAnimation = false)
    {
        for (int i = 1; i < puzzleBlocks.Count; i++)
        {
            puzzleBlocks[i].MarkAsCorrectBlock(i, noAnimation);
        }

        //PuzzleBlocksCentrify.Instance.RemovePuzzleBlock(puzzleBlocks[0], !noAnimation);

    }
    public void MarkWordAsFinished(string word)
    {
        var puzzleBlocks = wordLinkedPuzzleBlocks[word];
        puzzleBlocks[0].gameObject.SetActive(false);
        for (int i = 1; i < puzzleBlocks.Count; i++)
        {
            puzzleBlocks[i].MarkAsCorrectBlock(i, false);
        }

        //PuzzleBlocksCentrify.Instance.RemovePuzzleBlock(puzzleBlocks[0], true);

    }

    private void OnLetterDraggableRetuned(DraggableLetter draggableLetter)
    {
        if (draggableLetter.letter == correctCharacters[draggableLetter.placedAtLocation])
        {
            // CORRECT
            draggableLetter.placedCorrectly = true;

            emojiCrossWord.placedWords++;
            emptyPuzzleBlocks[draggableLetter.placedAtLocation].LoadAsNormalText(draggableLetter.letter);
            if (emojiCrossWord.currentLeftOverWords.Count > 0)
            {
                emojiCrossWord.currentLeftOverWords.Remove(draggableLetter.letter);
            }
            if (emojiCrossWord.currentLeftOverWords.Count == 0)
            {
                BringNextSetOfLetters();
            }
            draggableLetter.ActivateCorrectPlacementEffects(HighlightFinishedWord(draggableLetter.letter));
            if (finishedWords.Count == wordLinkedPuzzleBlocks.Count)
            {
                LevelFinished();
            }

            // save cross word
            letterBoxes[draggableLetter.placedAtLocation].isClue = false;
            SavePuzzle();

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
    public void RemoveClueForCrosswordblock(Vector2Int blockLocation)
    {
        letterBoxes[blockLocation].isClue = false;
        SavePuzzle();
    }

    public bool AllWordsFinished()
    {
        return unsolvedCrossWords.Count == 0;
    }

    public char[] GiveNextSetOfLetters()
    {
        return leftOverLetters
            .Skip(emojiCrossWord.placedWords)  // Skip the already placed letters
            .Take(maxDraggableLetters)     // Take the required number of letters
            .ToArray();                    // Convert the result to a char array
    }


    public List<PuzzleBlock> GetPuzzleBlocksLinkedForWord(string word)
    {
        return wordLinkedPuzzleBlocks[word];
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
                    foreach (var item in targetPuzzleBlocks)
                    {
                        item.partOfWords.Add(word);
                    }

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

        puzzleParent.DOScale(1.05f, .1f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            UIManager.Instance.ActivateConfetti();
            puzzleParent.DOScale(1f, .5f).SetEase(Ease.OutBack);
        });

        yield return new WaitForSeconds(.2f + .9f);

        UIManager.Instance.OutLevelHeaderInCoins();

        foreach (var blocks in wordLinkedPuzzleBlocks.Values)
        {
            var posMean = Vector2.zero;
            for (int i = 0; i < blocks.Count; i++)
            {
                posMean += (Vector2)blocks[i].rectTransform.localPosition;
                blocks[i].FadeOut(i);
            }
            posMean /= blocks.Count;

            CoinsGenerator.Instance.SpawnACoin(posMean);

            yield return new WaitForSeconds(.3f);
        }

        yield return new WaitForSeconds(1.5f);

        UIManager.Instance.OnAllCoinsFinished();
    }

}
