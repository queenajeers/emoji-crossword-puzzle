using System.Collections.Generic;
using UnityEngine;
using System.IO;



[System.Serializable]
public class EmojiCrossWord
{
    public string puzzleName;
    public string themeName;
    public Vector2Int gridSize;
    [SerializeReference] public List<DataBlock> dataBlocks = new List<DataBlock>();
    public List<string> crossWords = new List<string>();
    public List<char> totalLeftOverWords = new List<char>();
    public List<char> currentLeftOverWords = new List<char>();
    public int placedWords = 0;

    public void SaveCrossWordPuzzle(string path)
    {
        var jsonText = JsonUtility.ToJson(this);
        Debug.Log($"SAVING TO PATH {path}");
        File.WriteAllText(path, jsonText);
    }
    public EmojiCrossWord LoadFromSavedPath(string path)
    {

        if (File.Exists(path))
        {
            return JsonUtility.FromJson<EmojiCrossWord>(File.ReadAllText(path));
        }

        return null;
    }

    public EmojiCrossWord LoadFromSavedPathMobile(string levelName, PuzzleDifficulty puzzleDifficulty)
    {
        TextAsset textAsset = Resources.Load<TextAsset>($"Levels/{puzzleDifficulty}/{levelName}");
        if (textAsset != null)
        {
            return JsonUtility.FromJson<EmojiCrossWord>(textAsset.text);
        }

        return null;
    }

}


[System.Serializable]
public class DataBlock
{
    public Vector2Int blockLocation;
    public List<ArrowIndication> arrowIndications = new List<ArrowIndication>();

    public void AddDirection(ArrowIndication arrowIndication)
    {
        if (!arrowIndications.Contains(arrowIndication))
        {
            arrowIndications.Add(arrowIndication);
        }
    }

}

[System.Serializable]
public class LetterBox : DataBlock
{
    public char letter;
    public bool isClue;

    public LetterBox(char letter)
    {
        this.letter = letter;
    }
    public void UpdateLeter(char letter)
    {
        this.letter = letter;
    }
}

[System.Serializable]
public class Hint : DataBlock
{
    public string localPath;
    public string imageURL;
    public HintDirection hintDirection;

}
public class TextHint : DataBlock
{
    public string content;
    public HintDirection hintDirection;

}
public class DoubleTextHint : DataBlock
{
    public List<string> contents = new List<string> { "", "" };
    public HintDirection hintDirection;

}

public enum HintDirection
{
    NoDirection,
    Left,
    Right,
    Up,
    Down
}

public enum ArrowIndication
{
    None,
    FromTop,
    FromRight,
    FromBottom,
    FromLeft,
    FromLeftQuarter
}


public class EmojiCrossWordUtility
{
    public static void CenterDataBlocks(EmojiCrossWord emojiCrossWord)
    {
        if (emojiCrossWord == null || emojiCrossWord.dataBlocks.Count == 0)
            return;

        // Calculate bounds of existing DataBlocks
        int minX = int.MaxValue, maxX = int.MinValue;
        int minY = int.MaxValue, maxY = int.MinValue;

        foreach (var dataBlock in emojiCrossWord.dataBlocks)
        {
            minX = Mathf.Min(minX, dataBlock.blockLocation.x);
            maxX = Mathf.Max(maxX, dataBlock.blockLocation.x);
            minY = Mathf.Min(minY, dataBlock.blockLocation.y);
            maxY = Mathf.Max(maxY, dataBlock.blockLocation.y);
        }

        // Calculate the offset to center the blocks
        int width = maxX - minX + 1;
        int height = maxY - minY + 1;

        Vector2Int centerOffset = new Vector2Int(
            (emojiCrossWord.gridSize.x - width) / 2 - minX,
            (emojiCrossWord.gridSize.y - height) / 2 - minY
        );

        // Update DataBlock positions
        foreach (var dataBlock in emojiCrossWord.dataBlocks)
        {
            dataBlock.blockLocation += centerOffset;
        }
    }
}

