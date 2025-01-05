using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class DraggableLettersContainer : MonoBehaviour
{
    public int maxLetters;
    public GameObject letterSlotPrefab;

    List<RectTransform> slotPositions = new List<RectTransform>();


    public void InitialiseSlots(float gridSize)
    {
        for (int i = 0; i < maxLetters; i++)
        {
            var letterSlot = Instantiate(letterSlotPrefab, transform);
            letterSlot.GetComponent<RectTransform>().sizeDelta = Vector2.one * gridSize;
            slotPositions.Add(letterSlot.GetComponent<RectTransform>());
        }

    }

}
