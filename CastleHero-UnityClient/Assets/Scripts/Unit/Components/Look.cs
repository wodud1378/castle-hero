using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class Look
    {
        private readonly Transform _transform;
        
        public Look(Transform transform) => _transform = transform;
        
        public void At(Vector2 at)
        {
            var originScale = _transform.transform.localScale;
            float originX = Mathf.Abs(originScale.x);
            float scale = at.x <= 0 ? originX : -originX;

            _transform.localScale = new Vector3(scale, originScale.y, originScale.z);
        }

        public void At(Vector2 from, Vector2 target) => At(target - from);
    }
}