using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmojiCrossWord
{
    public Vector2Int gridSize;
    public List<DataBlock> dataBlocks;
    public List<CrossWord> crossWords;
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
public class Box : DataBlock
{
    public char letter;
    public bool isClue;
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


