using UnityEngine;
using UnityEngine.Rendering;

namespace CastleHero.Utility
{
    public static class ObjectHelper
    {
        public static void ToPreviewLayer(this GameObject obj) => obj.ToLayer("UnitPreview");

        public static void ToLayer(this GameObject obj, string layer) => obj.ToLayer(SortingLayer.NameToID(layer));

        public static void ToLayer(this GameObject obj, int layer)
        {
            if (!obj.TryGetComponent(out SortingGroup sortingGroup))
                return;

            sortingGroup.sortingLayerID = layer;
        }

        public static int GetLayer(this GameObject obj)
        {
            if (!obj.TryGetComponent(out SortingGroup sortingGroup))
                return 0;

            return sortingGroup.sortingLayerID;
        }

        public static Vector3 ScreenToWorld(this Vector2 screenPoint)
        {
            var camera = Camera.main;
            if (camera == null)
                return default;

            var position = camera.ScreenToWorldPoint(screenPoint);
            position.z = 0f;
            return position;
        }
    }
}