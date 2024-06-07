using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.InGame.Effects
{
    public interface IEffect
    {
        public float Duration { get; set; }
        
        public void Run(Vector2 startAt = default);
        public void Stop();
        public void SetTarget(UnitBehaviour unit);
        public void SetTarget(Vector2 position);
    }
}