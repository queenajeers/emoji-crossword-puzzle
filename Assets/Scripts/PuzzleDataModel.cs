using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class EmojiCrossWord
{
    public Vector2Int gridSize;
    public List<CrossWord> crossWords;
}


[System.Serializable]
public class CrossWord
{
    public string word; // HELLO
    public string clueWord;// __LL_
    public Vector2Int startingPosition;
    public CrossWordDirection crossWordDirection;
    public Hint hint;

}

[System.Serializable]
public class Hint
{
    public string imagePath;
    public HintLocation hintLocation;
}


public enum CrossWordDirection
{
    Left,
    Right,
    Up,
    Down,
}

public enum HintLocation
{
    Left,
    Right,
    Up,
    Down,
}