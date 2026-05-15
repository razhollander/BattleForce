using UnityEngine;

public static class TransformExtensions
{
    /// <summary>
    /// Sets the X and Y positions of a Transform using a Vector2, leaving the Z position unchanged.
    /// </summary>
    public static void SetPositionXY(this Transform transform, Vector2 newXY)
    {
        transform.position = new Vector3(newXY.x, newXY.y, transform.position.z);
    }

    /// <summary>
    /// Sets the X and Y anchored positions of a RectTransform using a Vector2.
    /// </summary>
    public static void SetAnchoredPositionXY(this RectTransform rectTransform, Vector2 newXY)
    {
        rectTransform.anchoredPosition = newXY;
    }
}