using UnityEngine;

namespace CastleHero.View.Common.UI.Utility
{
    public class ScaleWithAspect : MonoBehaviour
    {
        public float referenceWidth;
        public float referenceHeight;

        private float ReferenceRatio => referenceWidth / referenceHeight;

        private void Awake()
        {
            transform.localScale = Vector3.one * GetFactor();
        }

        public float GetFactor()
        {
            var width = (float)Screen.width;
            var height = (float)Screen.height;
            var ratio = width / height;

            return ratio > ReferenceRatio
                ? ReferenceRatio / ratio
                : 1f;
        }
    }
}