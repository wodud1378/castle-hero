using System.Collections.Generic;
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
        [SerializeField] private int[] _characterIds;
        
        private IUnitFactory _factory;
        private PlayerUnit[] _playerUnits;
        
        public async UniTask Init(UnitDB db)
        {
            _factory = new DefaultUnitFactory();
            
            await InitializeUnits(db);
        }
        
        private async UniTask InitializeUnits(UnitDB db)
        {
            int count = _characterIds.Length;
            _playerUnits = new PlayerUnit[count];

            var tasks = new UniTask[count];
            for (int i = 0; i < count; ++i)
            {
                if (!db.TryFind(_characterIds[i], out var entity))
                    continue;
                
                tasks[i] = CreatUnit(i, entity);
            }

            await UniTask.WhenAll(tasks);
        }
        
        private async UniTask CreatUnit(int index, UnitEntity entity)
        {
            var unit = await _factory.Create<PlayerUnit>(entity, Vector2.zero);
            _playerUnits[index] = unit;
        }
    }
}