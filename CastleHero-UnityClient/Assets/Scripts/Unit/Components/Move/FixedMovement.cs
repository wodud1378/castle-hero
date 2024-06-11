using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Finding;
using UnityEngine;

namespace RGLabs.Unit.Components.Move
{
    public class FixedMovement : IMovement
    {
        public bool Enabled { get; set; }
        
        public Vector2 Default { get; set; }
        public UnitBehaviour CurrentTarget { get; set; }
        public Finder Finder => null;

        public bool TryMoveToTarget() => false;

        public bool TryMoveToDefault() => false;

        public void Stop()
        {
        }
    }
}