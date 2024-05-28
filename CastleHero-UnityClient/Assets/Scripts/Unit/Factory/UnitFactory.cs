using Cysharp.Threading.Tasks;
using RGLabs.Common.Pattern;
using RGLabs.Data.DB;
using RGLabs.Data.User;
using RGLabs.Unit.Behaviours;
using UnityEngine;

namespace RGLabs.Unit.Factory
{
    public class UnitFactory : IUnitFactory
    {
        private readonly PoolContainer _container;
        private readonly UnitDB _unitDB;
        private readonly UnitLevelDB _levelDB;

        public UnitFactory(PoolContainer container, UnitDB unitDB, UnitLevelDB levelDB)
        {
            _container = container;
            _unitDB = unitDB;
            _levelDB = levelDB;
        }
        
        public async UniTask<UnitBehaviour> Create(int id, int lv, int grade, Vector2 position)
        {
            var info = new UnitInfo
            {
                lv = lv,
                grade = grade,
                id = id
            };

            return await Create(info, position);
        }

        public async UniTask<UnitBehaviour> Create(UnitInfo info, Vector2 position)
        {
            if (!_unitDB.TryFind(info.id, out var unitEntity))
                return null;

            var unit = await CreateInternal(unitEntity.prefab, position);
            if (unit == null)
                return null;

            if (!_levelDB.TryFind(info.id, out var levelEntity))
                levelEntity = default;
            
            unit.Init(info, unitEntity, levelEntity);
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