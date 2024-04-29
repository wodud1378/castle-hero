using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Common;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Model;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.Utility;
using UnityEngine;

namespace RGLabs.InGame.Behaviours
{
    [Serializable]
    public struct FieldCharacter
    {
        public int index;
        public Vector2 position;
    }

    public class Formation : MonoBehaviour
    {
        [field: SerializeField] public float Radius { get; private set; }

        [SerializeField] private int _defaultCastleId;

        private readonly Collider2D[] _buffer = new Collider2D[Constants.SpawnBufferSize];

        public IUnitFactory Factory { get; private set; }
        
        private UnitDB _db;
        private UserRepository _userRepo;
        private InGameRepository _gameRepo;

        public async UniTask Init(UnitDB db, UserRepository userRepo, InGameRepository gameRepo, IUnitFactory factory)
        {
            _db = db;
            _userRepo = userRepo;
            _gameRepo = gameRepo;
            Factory = factory;

            await LoadCastle();
            await LoadSavedUnits();
        }

        public void Remove(UnitBehaviour unit)
        {
            if (unit == null)
                return;

            if (_gameRepo.characters.Value == null)
                return;
            
            _gameRepo.characters.Value = _gameRepo.characters.Value
                .Where(x => x != unit)
                .ToArray();
            
            unit.DestroySelf();
        }

        public bool TryRegister(UnitBehaviour unit, int layer)
        {
            if (!unit.IsValid())
                return false;

            if (!IsValid(unit.Collider, layer))
                return false;

            Register(unit);
            return true;
        }

        public bool IsValid(Collider2D col, int layer)
        {
            var layerMask = new LayerMask { value = 1 << layer };
            var distance = Vector2.Distance(col.transform.position, transform.position);
            if (distance > Radius || distance < 1f)
                return false;

            return Physics2D.OverlapCollider(col, new ContactFilter2D { layerMask = layerMask }, _buffer) <= 0;
        }

        private void Register(UnitBehaviour unit)
        {
            var units = _gameRepo.characters.Value;
            units ??= Array.Empty<UnitBehaviour>();

            int index = Array.FindIndex(units, (x) => x.Id == unit.Id);
            if (index != -1)
            {
                units[index].DestroySelf();
                units[index] = null;
            }

            units = units
                .Where(x => x != null)
                .Append(unit)
                .ToArray();

            _gameRepo.characters.Value = units;
            _userRepo.SaveFieldCharacters(units);
        }

        private async UniTask LoadCastle()
        {
            if (!_db.TryFind(_userRepo.castle.Value, out var entity))
            {
                if (!_db.TryFind(_defaultCastleId, out entity))
                    return;
            }

            _userRepo.castle.Value = entity.Id;
            _gameRepo.castle.Value = await CreateUnit(entity, Vector2.zero);
        }

        private async UniTask LoadSavedUnits()
        {
            var tasks = new List<UniTask>();
            var characters = _userRepo.characters.Value;
            if (characters == null)
                return;

            var saved = _userRepo.fieldCharacters.Value;
            if (saved == null)
                return;

            foreach (var data in saved)
            {
                int index = data.index;
                if (!index.IsValidIndex(characters))
                    continue;

                var character = characters[index];
                if (!_db.TryFind(character.id, out var entity))
                    continue;

                tasks.Add(CreateCharacter(entity, data.position));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask CreateCharacter(UnitEntity entity, Vector2 position)
        {
            var unit = await CreateUnit(entity, position);
            Register(unit);
        }

        private async UniTask<UnitBehaviour> CreateUnit(UnitEntity entity, Vector2 position)
        {
            var unit = await entity.Create<UnitBehaviour>(position, Factory);
            unit.defaultDestination = position;
            unit.canMove = false;
            unit.canAttack = false;
            return unit;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}