using System;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.System.UnitFactory;
using UnityEngine;

namespace RGLabs.InGame.Behaviours.Player
{
    public class Camp : MonoBehaviour
    {
        public event Action OnDestroyed;
        
        [Serializable]
        public struct InitialUnit
        {
            public int id;
            public Vector2 position;
        }

        [SerializeField] private int _castleId;
        [SerializeField] private InitialUnit[] _initialUnits;

        private UnitBehaviour _castle;
        private UnitBehaviour[] _playerUnits;
        private IUnitFactory _factory;
        
        public async UniTask Init(UnitDB db)
        {
            _factory = new DefaultUnitFactory(InGameContext.Pools);
            
            await InitializeUnits(db);
        }

        private async UniTask InitializeUnits(UnitDB db)
        {
            int count = _initialUnits.Length;
            _playerUnits = new UnitBehaviour[count];

            var tasks = new UniTask[count + 1];
            for (int i = 0; i < count; ++i)
            {
                if (!db.TryFind(_initialUnits[i].id, out var entity))
                    continue;
                
                tasks[i] = CreatUnit(i, entity, _initialUnits[i].position);
            }
            
            if (db.TryFind(_castleId, out var castleEntity))
                tasks[count] = CreateCastle(castleEntity, Vector2.zero); 

            await UniTask.WhenAll(tasks);
        }
        
        private async UniTask<UnitBehaviour> CreatUnit(UnitEntity entity, Vector2 position)
        {
            var unit = await _factory.Create<UnitBehaviour>(entity, position);
            unit.defaultDestination = position;
            return unit;
        }
        
        private async UniTask CreateCastle(UnitEntity entity, Vector2 position)
        {
            _castle = await CreatUnit(entity, position);
            _castle.OnDead -= OnCastleDestroyed;
            _castle.OnDead += OnCastleDestroyed;
        }
        
        private async UniTask CreatUnit(int index, UnitEntity entity, Vector2 position)
        {
            _playerUnits[index] = await CreatUnit(entity, position);
        }

        private void OnCastleDestroyed(UnitBehaviour _) => OnDestroyed?.Invoke();

        private void OnDrawGizmosSelected()
        {
            if (_initialUnits == null)
                return;

            foreach (var initialUnit in _initialUnits)
            {
                Gizmos.DrawWireSphere(initialUnit.position, 0.5f);
            }
        }
    }
}