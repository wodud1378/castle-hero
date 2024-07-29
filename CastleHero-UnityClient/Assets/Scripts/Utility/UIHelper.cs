using UnityEngine;

namespace RGLabs.Utility
{
    public static class UIHelper
    {
        public static void AttachThrough(this RectTransform from, RectTransform to, float pivotX, float pivotY)
        {
            from.anchorMin = new Vector2(0f, 1f);
            from.anchorMax = new Vector2(0f, 1f);
            from.pivot = new Vector2(0f, 1f);
            from.anchoredPosition = to.anchoredPosition;
        }
    }
}