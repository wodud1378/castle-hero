using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data;
using RGLabs.Data.DB;
using RGLabs.Unit.Behaviours;
using RGLabs.Network.Shared;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class UnitFactory : IUnitFactory
    {
        private readonly PoolContainer _container = Storage.poolContainer;
        private readonly UnitDB _unitDB = Storage.db.units;
        private readonly UnitBalanceDB _balanceDB = Storage.db.balances;

        public async UniTask<UnitBehaviour> Create(int id, int lv, int grade, Vector2 position)
        {
            var info = new UnitInfo
            {
                lv = lv,
                rate = grade,
                id = id
            };

            return await Create(info, position);
        }

        public async UniTask<UnitBehaviour> CreateBarricade(UnitInfo info, Vector2 position)
        {
            int lv = Storage.userRepository.castleLv.Value;
            if (!Storage.db.castles.TryFind(lv, out var castleEntity))
                return null;
            
            if (!_unitDB.TryFind(info.id, out var unitEntity))
                return null;

            unitEntity.hp = castleEntity.barricadeHp;
            
            var unit = await CreateInternal(unitEntity.prefab, position);
            if (unit == null)
                return null;
            
            unit.Init(info, unitEntity, default);
            unit.position = position;
            return unit;
        }

        public async UniTask<UnitBehaviour> Create(UnitInfo info, Vector2 position)
        {
            if (!_unitDB.TryFind(info.id, out var unitEntity))
                return null;

            var unit = await CreateInternal(unitEntity.prefab, position);
            if (unit == null)
                return null;

            if (!_balanceDB.TryFind(info.id, out var balanceEntity))
                balanceEntity = default;
            
            unit.Init(info, unitEntity, balanceEntity);
            unit.position = position;
            return unit;
        }

        private async UniTask<UnitBehaviour> CreateInternal(string prefab, Vector2 position)
        {
            var unit = await _container.GetItem<UnitBehaviour>(prefab, position);
            if (unit == null)
            {
#if UNITY_EDITOR
                Debug.LogError($"[{prefab}] 리소스가 존재하지 않습니다.");
#endif
                return null;
            }

            return unit;
        }
    }
}