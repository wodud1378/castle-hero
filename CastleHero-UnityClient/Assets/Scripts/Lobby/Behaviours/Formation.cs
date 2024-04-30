using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.InGame;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.Behaviours
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

        private readonly Collider2D[] _buffer = new Collider2D[Constants.BufferSize];

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

            if (unit == _gameRepo.castle.Value)
                return;

            if (_gameRepo.characters.Value == null)
                return;
            
            _gameRepo.characters.Value = _gameRepo.characters.Value
                .Where(x => x.behaviour != unit)
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
            var characters = _gameRepo.characters.Value;
            characters ??= Array.Empty<InGameCharacter>();

            int index = Array.FindIndex(characters, (x) => x.character.id == unit.Id);
            if (index != -1)
            {
                characters[index].behaviour.DestroySelf();
                characters[index].behaviour = null;
            }

            var character = _userRepo.FindCharacter(unit.Id);
            characters = characters
                .Where(x => x.behaviour != null)
                .Append(new InGameCharacter
                {
                    character = character,
                    behaviour = unit
                })
                .ToArray();

            _gameRepo.characters.Value = characters;
            _userRepo.SaveFieldCharacters(characters);
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