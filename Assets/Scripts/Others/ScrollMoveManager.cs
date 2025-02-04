using UnityEngine;

public class ScrollMoveManager : MonoBehaviour
{
    public static ScrollMoveManager Instance { get; private set; }
    public RectTransform referenceAreaRect;
    void Awake()
    {
        Instance = this;
    }

    public void PuzzleBlockAdded(GameObject puzzleBlock)
    {
        var smi = puzzleBlock.AddComponent<ScrollMoveItem>();
        smi.AssignScrollManager(this);
    }
}
