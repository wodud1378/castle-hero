using UnityEngine;

namespace RGLabs.Utility
{
    public static class UIHelper
    {
        public static void Attach(this RectTransform obj, RectTransform target, Vector2 objPivot)
        {    
            obj.anchorMin = objPivot;
            obj.anchorMax = objPivot;
            obj.pivot = objPivot;
            obj.position = target.position;
        }
    }
}