using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Unit.Finding
{
    public interface IDetection
    {
        public enum Option
        {
            Circle,
            Arc,
            Box,
        }
        
        public Collider2D[] Buffer { get; set; }
        public LayerMask Filter { get; set; }
        public int MaxTarget { get; set; }

        public void SetRange(float x, float y);

        public bool TrySearch(Vector2 position, out int found);

        public void SetAngle(float angle);
        public void SetForward(Vector2 forward);
    }

    public class BoxDetection : IDetection
    {
        private Vector2 _range;

        public Collider2D[] Buffer { get; set; }
        public LayerMask Filter { get; set; }
        public int MaxTarget { get; set; }

        private float _angle;
        
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
            found = Physics2D.OverlapBoxNonAlloc(position, _range, _angle, Buffer, Filter);
            if (MaxTarget > 0)
                found = Mathf.Min(found, MaxTarget);

            return found > 0;
        }

        public void SetAngle(float angle) => _angle = angle;

        public void SetForward(Vector2 forward) => _angle = forward.ToFloat();
    }

    public class CircleDetection : IDetection
    {
        private float _radius;

        public Collider2D[] Buffer { get; set; }
        public LayerMask Filter { get; set; }
        public int MaxTarget { get; set; }
        
        public float Angle { get; set; }
        public Vector2 Forward { get; set; }
        
        protected float angle;
        protected Vector2 forward;

        public void SetRange(float range)
        {
            _radius = range;
        }

        public void SetRange(float x, float y)
        {
            _radius = (x + y) * 0.5f;
        }
        
        public virtual bool TrySearch(Vector2 position, out int found)
        {
            found =  Physics2D.OverlapCircleNonAlloc(position, _radius, Buffer, Filter);
            if (MaxTarget > 0)
                found = Mathf.Min(found, MaxTarget);

            return found > 0;
        }

        public void SetAngle(float angle) => this.angle = angle;

        public void SetForward(Vector2 forward) => this.forward = forward;
    }

    public class ArcDetection : CircleDetection
    {
        private readonly Vector2[] _arcCheckBuffer = new Vector2[4];
 
        public override bool TrySearch(Vector2 position, out int found)
        {
            if (!base.TrySearch(position, out found))
                return false;

            int validCount = 0;
            for (int i = 0; i < found; ++i)
            {
                if (!InBound(position, Buffer[i]))
                    continue;
                
                Buffer[validCount++] = Buffer[i];
            }

            found = validCount;
            return found > 0;
        }
        
        private bool InBound(Vector2 point, Collider2D collider)
        {
            if (collider == null)
                return false;

            Vector2 targetPos = (Vector2)collider.transform.position - point;
            Vector2 halfSize = collider.bounds.size * 0.5f;

            // Left Up
            _arcCheckBuffer[0].Set(targetPos.x - halfSize.x, targetPos.y + halfSize.y);
            // Right Up
            _arcCheckBuffer[1].Set(targetPos.x + halfSize.x, targetPos.y + halfSize.y);
            // Left Bottom
            _arcCheckBuffer[2].Set(targetPos.x - halfSize.x, targetPos.y - halfSize.y);
            // Right Bottom
            _arcCheckBuffer[3].Set(targetPos.x + halfSize.x, targetPos.y - halfSize.y);

            float halfAngle = angle * 0.5f;
            foreach (var buffer in _arcCheckBuffer)
            {
                float angle = Vector2.Angle(buffer, forward);
                if (angle <= halfAngle)
                    return true;
            }

            return false;
        }
    }
}