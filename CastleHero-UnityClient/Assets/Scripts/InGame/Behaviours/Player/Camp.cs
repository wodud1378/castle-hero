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
        [Serializable]
        public struct InitialUnit
        {
            public int id;
            public Vector2 position;
        }

        [SerializeField] private UnitBehaviour _castle;
        [SerializeField] private int _castleId;
        
        [SerializeField] private InitialUnit[] _initialUnits;
        
        private IUnitFactory _factory;
        private UnitBehaviour[] _playerUnits;
        
        public async UniTask Init(UnitDB db)
        {
            _factory = new DefaultUnitFactory(InGameContext.Pools);
            
            await InitializeUnits(db);
        }
        
        private async UniTask InitializeUnits(UnitDB db)
        {
            if(db.TryFind(_castleId, out var castleEntity))
                _castle.Init(castleEntity);
            
            int count = _initialUnits.Length;
            _playerUnits = new UnitBehaviour[count];

            var tasks = new UniTask[count];
            for (int i = 0; i < count; ++i)
            {
                if (!db.TryFind(_initialUnits[i].id, out var entity))
                    continue;
                
                tasks[i] = CreatUnit(i, entity, _initialUnits[i].position);
            }

            await UniTask.WhenAll(tasks);
        }
        
        private async UniTask CreatUnit(int index, UnitEntity entity, Vector2 position)
        {
            var unit = await _factory.Create<UnitBehaviour>(entity, position);
            unit.defaultDestination = position;
            _playerUnits[index] = unit;
        }

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