using UnityEngine;

namespace RGLabs.InGame.Unit
{
    public class ThrustAlley : FindUnits
    {
        private readonly Rigidbody2D _rigidbody;
        
        public ThrustAlley(Rigidbody2D rigidbody, LayerMask layerMask, RaycastHit2D[] castBuffer, int maxTarget) 
            : base(layerMask, castBuffer, maxTarget)
        {
            
            _rigidbody = rigidbody;
        }

        protected override bool OnUpdate(int found)
        {
            if (found == 0)
                return false;

            for (int i = 0; i < found; ++i)
            {
                if (!TryGetUnit(_castBuffer[i], out var unit))
                    continue;
                
                var point = unit.Position - _rigidbody.position;
                var direction = point.normalized;
                unit.Body.AddForceAtPosition(direction * _rigidbody.mass, point, ForceMode2D.Impulse);
            }
            
            return true;
        }
    }
}