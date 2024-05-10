using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UIAtlasedSpriteCollection : MonoBehaviour
    {
        public UIAtlasedSprite[] sprites;

        private void OnEnable()
        {
            foreach (var sprite in sprites)
            {
                sprite.enabled = true;
            }
        }

        private void OnDisable()
        {
            foreach (var sprite in sprites)
            {
                sprite.enabled = false;
            }
        }
    }
}