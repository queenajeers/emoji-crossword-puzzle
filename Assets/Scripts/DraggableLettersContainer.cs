using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DraggableLettersContainer : MonoBehaviour
{
    public static DraggableLettersContainer Instance { get; private set; }
    public Transform lettersDraggableParent;
    public GameObject letterSlotPrefab;
    public GameObject letterDraggablePrefab;

    List<RectTransform> slotPositions = new List<RectTransform>();

    public float letterBoxSize;

    void Awake()
    {
        Instance = this;
    }

    public void InitialiseSlots(int maxLetters)
    {
        for (int i = 0; i < maxLetters; i++)
        {
            var letterSlot = Instantiate(letterSlotPrefab, transform);
            letterSlot.GetComponent<RectTransform>().sizeDelta = Vector2.one * letterBoxSize;
            slotPositions.Add(letterSlot.GetComponent<RectTransform>());

        }

    }


    public void LoadLetters(char[] chars)
    {
        lettersDraggableParent.SetAsLastSibling();
        for (int i = 0; i < chars.Length; i++)
        {
            var letterDraggable = Instantiate(letterDraggablePrefab, lettersDraggableParent);
            letterDraggable.GetComponent<RectTransform>().sizeDelta = Vector2.one * letterBoxSize;
            letterDraggable.GetComponent<RectTransform>().localPosition = slotPositions[i].localPosition;
            letterDraggable.GetComponent<DraggableLetter>().LoadOriginalPosition();
            letterDraggable.GetComponent<DraggableLetter>().LoadOriginalSizes();
            letterDraggable.GetComponent<DraggableLetter>().AssignLetter(chars[i]);
            letterDraggable.GetComponent<DraggableLetter>().PopIn(i * .1f);

        }
    }

}
