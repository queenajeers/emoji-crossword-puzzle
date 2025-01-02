using System.Collections.Generic;
using UnityEngine;
using System.IO;



[System.Serializable]
public class EmojiCrossWord
{
    public string puzzleName;
    public Vector2Int gridSize;
    [SerializeReference] public List<DataBlock> dataBlocks = new List<DataBlock>();
    public List<CrossWord> crossWords = new List<CrossWord>();

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

}


[System.Serializable]
public class CrossWord
{

    public string word; // HELLO
    public string maskedWord;// __LL_
    public Vector2Int startingPosition;
    public CrossWordDirection crossWordDirection;

}


public enum CrossWordDirection
{
    Left,
    Right,
    Up,
    Down,
}

[System.Serializable]
public class DataBlock
{
    public Vector2Int blockLocation;
    public ArrowIndication arrowIndication;

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
    FromLeft
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

