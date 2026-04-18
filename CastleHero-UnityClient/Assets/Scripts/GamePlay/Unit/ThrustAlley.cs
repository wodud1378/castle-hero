using CastleHero.GamePlay.Unit.Behaviours;
using UnityEngine;

namespace CastleHero.GamePlay.Unit
{
    public class ThrustAlley
    {
        private readonly UnitActor _root;

        public ThrustAlley(UnitActor root) => _root = root;
        
        public void Execute(Collision2D collision)
        {
            foreach (var contact in collision.contacts)
            {
                var obj = contact.collider.gameObject;
                if (!obj.CompareTag(_root.tag))
                    continue;

                if (obj.layer != _root.gameObject.layer)
                    continue;

                var rigidbody = contact.rigidbody;
                if (rigidbody == null)
                    continue;

                var point = contact.point - _root.Position;
                var direction = point.normalized;
                rigidbody.AddForceAtPosition(direction * 1.5f, point, ForceMode2D.Force);
            }
        }
    }
}