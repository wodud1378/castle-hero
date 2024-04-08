using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.System.UnitFactory
{
    public interface IUnitFactory
    {
        public UniTask<T> Create<T>(UnitEntity entity, Vector2 position) where T : UnitBehaviour;
    }
}