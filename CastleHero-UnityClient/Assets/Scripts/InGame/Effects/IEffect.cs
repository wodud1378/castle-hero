using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.InGame.Effects
{
    public interface IEffect
    {
        public void Run();
        public void Stop();
        public void SetTarget(UnitBehaviour unit);
        public void SetTarget(Vector2 position);
    }
}