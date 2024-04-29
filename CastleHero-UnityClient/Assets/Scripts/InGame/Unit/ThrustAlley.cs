using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public class ThrustAlley : FindUnits
    {
        private readonly Collider2D _collider;
        private readonly Rigidbody2D _rigidbody;
        private readonly ContactFilter2D _contactFilter;

        public ThrustAlley(Collider2D collider, Rigidbody2D rigidbody, Collider2D[] castBuffer, int maxTarget)
            : base(castBuffer, maxTarget)
        {
            _collider = collider;
            _rigidbody = rigidbody;

            _contactFilter = new ContactFilter2D
            {
                useLayerMask = true,
                layerMask = new LayerMask { value = 1 << _collider.gameObject.layer }
            };
        }

        protected override bool TrySearch(Vector2 position, float range, out int found)
        {
            found =  Physics2D.OverlapCollider(_collider, _contactFilter, _castBuffer);
            if (_maxTarget > 0)
                found = Mathf.Min(found, _maxTarget);

            return found > 0;
        }

        protected override bool OnUpdate(int found)
        {
            if (found == 0)
                return false;

            for (int i = 0; i < found; ++i)
            {
                if (!TryGetUnit(_castBuffer[i], out var unit))
                    continue;

                var point = unit.position - _rigidbody.position;
                var direction = point.normalized;
                unit.Body.AddForceAtPosition(direction * _rigidbody.mass, point, ForceMode2D.Impulse);
            }

            return true;
        }
    }
}