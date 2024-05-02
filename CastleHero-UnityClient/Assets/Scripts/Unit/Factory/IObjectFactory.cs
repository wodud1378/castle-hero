using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public interface IObjectFactory<TObject, in TData> where TData : IEntity
    {
        public UniTask<TObject> Create(TData entity, Vector2 position);
    }
    
    public interface IUnitFactory : IObjectFactory<UnitBehaviour, UnitEntity>
    {
    }
}