using System.Collections.Generic;
using UnityEngine;


public class UIUtils
{
    public static Vector2 GetCenterAnchorUIPos(RectTransform target, Transform parentPanel)
    {
        // Get the world position of the grid element
        Vector3 worldPosition = target.position;

        // Convert world position to the panel's local position
        Vector3 localPosition = parentPanel.InverseTransformPoint(worldPosition);

        // Set the anchored position of the image
        return localPosition;
    }
    public static void Shuffle<T>(List<T> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
}

