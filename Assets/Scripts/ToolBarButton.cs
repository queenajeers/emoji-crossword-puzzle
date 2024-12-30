using System;
using UnityEngine;
using UnityEngine.UI;

public class ToolBarButton : MonoBehaviour
{
    public PuzzleCreatorBrush brushType;
    public Outline selectOutline;
    public static Action<PuzzleCreatorBrush> OnBrushSelected;


    public void SelectButton()
    {
        selectOutline.effectDistance = new Vector2(4, -4);

    }
    public void DeSelectButton()
    {
        selectOutline.effectDistance = new Vector2(0, 0);
    }

    public void SelectThisBrush()
    {
        OnBrushSelected?.Invoke(brushType);
    }
}
