using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.Network.Shared;
using UnityEngine;

namespace CastleHero.Data.Factory
{
    public interface IUnitFactory
    {
        UniTask<IUnitBehaviour> Create(int id, int lv, int grade, Vector2 position);
        UniTask<IUnitBehaviour> Create(UnitInfo info, Vector2 position);
        UniTask<IUnitBehaviour> CreateBarricade(UnitInfo info, Vector2 position);
    }
}
