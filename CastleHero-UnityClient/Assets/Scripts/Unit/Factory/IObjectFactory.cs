using Cysharp.Threading.Tasks;
using RGLabs.Network.Model;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public interface IUnitFactory
    {
        public UniTask<UnitBehaviour> Create(int id, int lv, int grade, Vector2 position);

        public UniTask<UnitBehaviour> Create(UnitInfo info, Vector2 position);
    }
}