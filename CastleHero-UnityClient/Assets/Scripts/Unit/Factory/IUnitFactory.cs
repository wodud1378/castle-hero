using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public interface IUnitFactory
    {
        public UniTask<T> Create<T>(UnitEntity entity, Vector2 position) where T : UnitBehaviour;
    }
}