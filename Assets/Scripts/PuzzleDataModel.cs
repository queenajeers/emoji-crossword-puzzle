using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class EmojiCrossWord
{
    public string puzzleName;
    public Vector2Int gridSize;
    public List<DataBlock> dataBlocks;
    public List<CrossWord> crossWords;

    public void SaveCrossWordPuzzle(string path)
    {
        var jsonText = JsonUtility.ToJson(this);
        File.WriteAllText(path, jsonText);
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
    public void SetAsClueLetter()
    {
        isClue = true;
    }
    public void SetAsNormalLetter()
    {
        isClue = false;
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


