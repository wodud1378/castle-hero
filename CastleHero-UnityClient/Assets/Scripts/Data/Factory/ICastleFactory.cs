using CastleHero.Common.Behaviours;
using CastleHero.Network.Shared;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace CastleHero.Data.Factory
{
    public interface ICastleFactory : IUnitFactory
    {
        UniTask<IUnitBehaviour> Create(string prefab, UnitInfo info, Vector2 position);
    }
}
