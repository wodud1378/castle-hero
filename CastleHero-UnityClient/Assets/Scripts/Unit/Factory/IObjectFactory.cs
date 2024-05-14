using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Data.User;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public interface IObjectFactory<TObject, in TData> where TData : IEntity
    {
        public UniTask<TObject> Create(TData entity, Vector2 position);
    }
    
    public interface IUnitFactory// : IObjectFactory<UnitBehaviour, UnitEntity>
    {
        public UniTask<UnitBehaviour> Create(int id, int lv, int grade, Vector2 position);

        public UniTask<UnitBehaviour> Create(UnitInfo info, Vector2 position);
    }
}