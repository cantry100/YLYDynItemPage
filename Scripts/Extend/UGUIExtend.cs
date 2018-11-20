using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// UGUI¿©’π
/// author: ”ÍLu“¢
/// </summary>
public static class UGUIExtend
{
    public static void SetAlpha(this Graphic obj, float alpha)
    {
        if (obj)
        {
            Color color = obj.color;
            color.a = alpha;
            obj.color = color;
        }
    }

    public static float GetAlpha(this Graphic obj)
    {
        if (obj)
        {
            Color color = obj.color;
            return color.a;
        }

        return 0f;
    }

    private static readonly Vector3[] m_Corners = new Vector3[4];
    public static Bounds CalculateRelativeRectTransformBounds(RectTransform root, RectTransform child)
    {
        if (child == null)
            return new Bounds();
        child.GetWorldCorners(m_Corners);
        var rootWorldToLocalMatrix = root.worldToLocalMatrix;
        return InternalGetBounds(m_Corners, ref rootWorldToLocalMatrix);
    }

    internal static Bounds InternalGetBounds(Vector3[] corners, ref Matrix4x4 rootWorldToLocalMatrix)
    {
        var vMin = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        var vMax = new Vector3(float.MinValue, float.MinValue, float.MinValue);

        for (int j = 0; j < 4; j++)
        {
            Vector3 v = rootWorldToLocalMatrix.MultiplyPoint3x4(corners[j]);
            vMin = Vector3.Min(v, vMin);
            vMax = Vector3.Max(v, vMax);
        }

        var bounds = new Bounds(vMin, Vector3.zero);
        bounds.Encapsulate(vMax);
        return bounds;
    }
}
