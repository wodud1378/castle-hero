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

        private UnitDB _db;
        private UserRepository _userRepo;
        private InGameRepository _gameRepo;
        private IUnitFactory _factory;

        public async UniTask Init(UnitDB db, UserRepository userRepo, InGameRepository gameRepo, IUnitFactory factory)
        {
            _db = db;
            _userRepo = userRepo;
            _gameRepo = gameRepo;
            _factory = factory;

            await LoadCastle();
            await LoadSavedUnits();
        }

        public bool TryRegister(UnitBehaviour unit)
        {
            if (!unit.IsValid())
                return false;

            if (!IsValid(unit.Collider))
                return false;

            Register(unit);
            return true;
        }

        public bool IsValid(Collider2D col)
        {
            int layer = col.gameObject.layer;
            var layerMask = new LayerMask { value = 1 << layer };
            var distance = Vector2.Distance(col.transform.position, transform.position);
            if (distance > Radius || distance < 1f)
                return false;

            return Physics2D.OverlapCollider(col, new ContactFilter2D { layerMask = layerMask }, _buffer) <= 0;
        }

        private void Register(UnitBehaviour unit)
        {
            var units = _gameRepo.characters.Value;
            int index = Array.FindIndex(units, (x) => x.Id == unit.Id);
            if (!index.IsValidIndex(units))
            {
                int length = units.Length;
                index = length;
                Array.Resize(ref units, length + 1);
            }

            units[index] = unit;
            
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
                if (index.IsValidIndex(characters))
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
            var unit = await entity.Create<UnitBehaviour>(position, _factory);
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