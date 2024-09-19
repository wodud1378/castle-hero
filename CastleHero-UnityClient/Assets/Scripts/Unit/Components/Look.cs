using System;
using Spine;
using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class Look
    {
        private readonly Transform _transform;
        private readonly Skeleton _skeleton;
        private readonly Action<Vector2> _action;
        
        public Look(Skeleton skeleton)
        {
            _skeleton = skeleton;
            _action = BySkeleton;
        }

        public Look(Transform transform)
        {
            _transform = transform;
            _action = ByTransform;
        }
        public void At(Vector2 from, Vector2 target) => _action.Invoke(target - from);

        private void BySkeleton(Vector2 at)
        {
            float scale = at.x <= 0 ? 1f : -1f;

            _skeleton.ScaleX = scale;
        }

        private void ByTransform(Vector2 at)
        {
            var originScale = _transform.localScale;
            float originX = Mathf.Abs(originScale.x);
            float scale = at.x <= 0 ? originX : -originX;

            _transform.localScale = new Vector3(scale, originScale.y, originScale.z);
        }
    }
}