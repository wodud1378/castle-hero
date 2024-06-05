using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnitInfo = RGLabs.Network.Model.UnitInfo;

namespace RGLabs.Lobby.Behaviours
{
    public class Formation : MonoBehaviour
    {
        [field: SerializeField] public float Radius { get; private set; }

        [SerializeField] private int _defaultCastleId;
        [SerializeField] private float _autoPlacementRadius;

        
        private readonly Collider2D[] _buffer = new Collider2D[Constants.BufferSize];

        private IUnitFactory _castleFactory;
        private IUnitFactory _unitFactory;

        private UserRepository _userRepo;
        private InGameRepository _gameRepo;

        public readonly ReactiveProperty<int> capacity = new();
        public readonly ReactiveProperty<int> placed = new();
        
        private int _barricadeCountMax;

        public async UniTask Init()
        {
            _userRepo = Storage.userRepository;
            _gameRepo = Storage.inGameRepository;
            _castleFactory = Storage.castleFactory;
            _unitFactory = Storage.unitFactory;

            _gameRepo.characters
                .ChangeAsObservable()
                .Subscribe(_userRepo.ApplyFieldCharacters)
                .AddTo(this);

            if (Storage.db.castles.TryFind(_userRepo.castleLv.Value, out var entity))
            {
                capacity.Value = entity.maxCharacter;
                _barricadeCountMax = entity.barricadeCount;
            }

            await LoadCastle();
            await LoadSavedUnits();
        }

        public async void AutoPlacement()
        {
            Clear();

            var characters = _userRepo.characters;
            int max = Mathf.Min(capacity.Value, characters.Count);
            int current = 0;
            float anglePerOnce = 360f / max;
            using var itr = characters.GetEnumerator();
            var tasks = new List<UniTask>();
            while (itr.MoveNext() && current < max)
            {
                var pos = (anglePerOnce * current++ + 90f).ToVector() * _autoPlacementRadius;
                tasks.Add(CreateCharacter(itr.Current, pos));
            }

            await UniTask.WhenAll(tasks);
        }

        public void Clear()
        {
            foreach (var unit in _gameRepo.characters)
            {
                unit.DestroySelf();
            }

            _gameRepo.characters.Clear();
        }

        public void Remove(UnitBehaviour unit)
        {
            if (unit == null)
                return;

            if (unit == _gameRepo.castle.Value)
                return;

            var characters = _gameRepo.characters;
            characters.Remove(unit);
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
            RemoveIfLimited(unit);

            _gameRepo.characters.Add(unit);
        }

        private void RemoveIfLimited(UnitBehaviour unit)
        {
            int limit = unit.Type == UnitBehaviour.BehaviourType.Barricade ? _barricadeCountMax : 1;
            var queue = new Queue<UnitBehaviour>();
            var characters = _gameRepo.characters;
            foreach (var character in characters)
            {
                if (character.Id == unit.Id && character != unit)
                    queue.Enqueue(character);
            }

            int count = limit <= queue.Count ? limit : queue.Count;
            while (count > 0)
            {
                var character = queue.Dequeue();
                characters.Remove(character);
                character.DestroySelf();

                --count;
            }
        }

        private async UniTask LoadCastle()
        {
            int lv = _userRepo.castleLv.Value;
            var unit = await _castleFactory.Create(1, lv, 0, Vector2.zero);
            _gameRepo.castle.Value = unit;
        }

        private async UniTask LoadSavedUnits()
        {
            var tasks = new List<UniTask>();
            foreach (var data in _userRepo.fieldCharacters)
            {
                int index = data.index;
                if (!index.IsValidIndex(_userRepo.characters))
                    continue;

                var character = _userRepo.characters[index];
                tasks.Add(CreateCharacter(character, data.position));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask CreateCharacter(UnitInfo info, Vector2 position)
        {
            var unit = await _unitFactory.Create(info, position);
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