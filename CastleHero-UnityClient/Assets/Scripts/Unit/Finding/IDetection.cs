using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public interface IDetection
    {
        public Collider2D[] Buffer { get; set; }
        public LayerMask Mask { get; set; }
        public int MaxTarget { get; set; }
        public float Angle { get; set; }

        public void SetRange(float range);
        public void SetRange(float x, float y);

        public bool TrySearch(Vector2 position, out int found);
    }

    public class BoxDetection : IDetection
    {
        private Vector2 _range;

        public Collider2D[] Buffer { get; set; }
        public LayerMask Mask { get; set; }
        public float Angle { get; set; }
        public int MaxTarget { get; set; }

        public void SetRange(float range)
        {
            _range = new Vector2(range, range);
        }

        public void SetRange(float x, float y)
        {
            _range = new Vector2(x, y);
        }

        public bool TrySearch(Vector2 position, out int found)
        {
            found = Physics2D.OverlapBoxNonAlloc(position, _range, Angle, Buffer, Mask);
            if (MaxTarget > 0)
                found = Mathf.Min(found, MaxTarget);

            return found > 0;
        }
    }

    public class CircleDetection : IDetection
    {
        private float _radius;

        public Collider2D[] Buffer { get; set; }
        public LayerMask Mask { get; set; }
        public int MaxTarget { get; set; }
        
        public float Angle { get; set; }

        public void SetRange(float range)
        {
            _radius = range;
        }

        public void SetRange(float x, float y)
        {
            _radius = (x + y) * 0.5f;
        }
        
        public bool TrySearch(Vector2 position, out int found)
        {
            found =  Physics2D.OverlapCircleNonAlloc(position, _radius, Buffer, Mask);
            if (MaxTarget > 0)
                found = Mathf.Min(found, MaxTarget);

            return found > 0;
        }
    }
}