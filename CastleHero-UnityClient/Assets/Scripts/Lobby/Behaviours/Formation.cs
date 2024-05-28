using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Data.DB;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Data.User;
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

        public IUnitFactory CastleFactory { get; private set; }
        public IUnitFactory UnitFactory { get; private set; }
        
        private UserRepository _userRepo;
        private InGameRepository _gameRepo;

        public async UniTask Init(UserRepository userRepo, InGameRepository gameRepo,
            IUnitFactory castleFactory, IUnitFactory unitFactory)
        {
            _userRepo = userRepo;
            _gameRepo = gameRepo;

            CastleFactory = castleFactory;
            UnitFactory = unitFactory;

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
                .Where(x => x.Id != unit.Id)
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
            characters ??= Array.Empty<UnitBehaviour>();

            int current = Array.FindAll(characters, (character) => character.Id == unit.Id).Length;
            if (current >= limit)
            {
                int index = Array.FindIndex(characters, (x) => x.Id == unit.Id);
                if (index != -1)
                {
                    var behaviour = characters[index];
                    if (behaviour != unit)
                        behaviour.DestroySelf();

                    characters[index] = null;
                }
            }

            characters = characters
                .Where(x => x != null)
                .Append(unit)
                .ToArray();

            _gameRepo.characters.Value = characters;
            _userRepo.SaveFieldCharacters(characters);
        }

        private async UniTask LoadCastle()
        {
            int lv = _userRepo.castleLv.Value;
            var unit = await CastleFactory.Create(1, lv, 0, Vector2.zero);
            _gameRepo.castle.Value = unit;
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
                tasks.Add(CreateCharacter(character, data.position));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask CreateCharacter(UnitInfo info, Vector2 position)
        {
            var unit = await UnitFactory.Create(info, position);
            if (unit == null)
                return;

            unit.Core.movement.Default = position;
            unit.Core.inBattle = false;

            Register(unit);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}