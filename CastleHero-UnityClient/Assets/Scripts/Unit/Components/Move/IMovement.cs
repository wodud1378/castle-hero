using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using UnityEngine;

namespace RGLabs.Unit.Components.Move
{
    public interface IMovement
    {
        public bool Enabled { get; set; }
        
        public Vector2 Default { get; set; }
        
        public UnitBehaviour CurrentTarget { get; }
        
        public Finder Finder { get; }
        
        public Vector2 Position { get; set; }

        public bool TryMoveToTarget();

        public bool TryMoveToDefault();
        
        public void Stop();

        public void SetSpeed(float value);
    }
}