using UnityEngine;

namespace RGLabs.Unit.Components
{
    public class Look : MonoBehaviour
    {
        public void At(Vector2 from, Vector2 target)
        {
            if (transform == null)
                return;

            var diff = target - from;
            var originScale = transform.transform.localScale;
            float originX = Mathf.Abs(originScale.x);
            float scale = diff.x <= 0 ? originX : -originX;

            transform.localScale = new Vector3(scale, originScale.y, originScale.z);
        }
    }
}