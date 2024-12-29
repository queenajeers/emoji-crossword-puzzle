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
}

