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
            
            var characters = _gameRepo.characters.Value
                .Where(x => x.behaviour.Id != unit.Id)
                .ToArray();

            _gameRepo.characters.Value = characters;
            _userRepo.SaveFieldCharacters(characters);
            
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
            bool isValid;
            if (distance > Radius)
            {
                isValid = false;
            }
            else
            {
                int overlapped = Physics2D.OverlapCollider(col, new ContactFilter2D { layerMask = layerMask }, _buffer);
                isValid = overlapped <= 0;
            }

            return isValid;
        }

        public bool InArea(Vector2 position) => Vector2.Distance(transform.position, position) <= Radius;

        private void Register(UnitBehaviour unit)
        {
            int limit = unit.Type == UnitBehaviour.BehaviourType.Barricade ? 3 : 1;
            var characters = _gameRepo.characters.Value;
            characters ??= Array.Empty<InGameCharacter>();
            
            int current = Array.FindAll(characters, (character) => character.behaviour.Id == unit.Id).Length;
            if (current >= limit)
            {
                int index = Array.FindIndex(characters, (x) => x.character.id == unit.Id);
                if (index != -1)
                {
                    var behaviour = characters[index].behaviour;
                    if (behaviour != unit)
                        behaviour.DestroySelf();
                    
                    characters[index].behaviour = null;
                }
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
            var unit = await Factory.Create(entity, position);
            unit.Core.defaultDestination = position;
            unit.CanMove = true;
            return unit;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}