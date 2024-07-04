using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using PolyNav;
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

        [SerializeField] private PolyNavMap _map;
        [SerializeField] private int _defaultCastleId;
        [SerializeField] private float _autoPlacementRadius;

        private readonly Collider2D[] _buffer = new Collider2D[Constants.BufferSize];

        private IUnitFactory _castleFactory;
        private UnitFactory _unitFactory;

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
                .Subscribe(OnFieldCharacterCollectionChanged)
                .AddTo(this);

            if (Storage.db.castles.TryFind(_userRepo.castleLv.Value, out var entity))
            {
                capacity.Value = entity.maxCharacter;
                _barricadeCountMax = entity.barricadeCount;
            }

            await LoadCastle();
            await LoadSavedUnits();

            var barricades = _gameRepo.characters
                .Where(x => x.Id == Constants.BarricadeId)
                .ToArray();
            
            if (barricades.Length > 0)
            {
                foreach (var barricade in barricades)
                {
                    _map.AddObstacle(barricade, false);
                }
                
                _map.GenerateMap();
            }
        }

        public async void AutoPlacement()
        {
            Clear();

            var characters = _userRepo.characters
                .Where(x => x.id != Constants.BarricadeId)
                .ToList();

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
            
            _userRepo.ApplyFieldCharacters(_gameRepo.characters);
        }

        public void Clear()
        {
            foreach (var unit in _gameRepo.characters)
            {
                unit.DestroySelf();
            }

            _gameRepo.characters.Clear();
            _userRepo.ApplyFieldCharacters(_gameRepo.characters);
        }

        public void Remove(UnitBehaviour unit)
        {
            if (unit == null)
                return;

            if (unit == _gameRepo.castle.Value)
                return;

            if(unit.Id == Constants.BarricadeId)
                _map.RemoveObstacle(unit);
                
            var characters = _gameRepo.characters;
            characters.Remove(unit);
            unit.DestroySelf();
            
            _userRepo.ApplyFieldCharacters(_gameRepo.characters);
        }

        public bool TryRegister(UnitBehaviour unit, int layer, bool isExist)
        {
            if (!unit.IsValid())
                return false;
            
            if (!IsValid(unit.Collider, layer))
                return false;

            bool isBarricade = unit.Id == Constants.BarricadeId;
            var characters = _gameRepo.characters;
            if (isExist)
            {
                int index = characters.IndexOf(unit);
                characters[index].position = unit.position;
            }
            else
            {
                if (!isBarricade &&
                    placed.Value >= capacity.Value)
                    return false;
                
                RemoveIfLimited(unit);
                characters.Add(unit);   
            }
            
            _userRepo.ApplyFieldCharacters(characters);
            
            if(isBarricade)
                _map.AddObstacle(unit);
            
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

        private void OnFieldCharacterCollectionChanged(ReactiveCollection<UnitBehaviour> collection)
        {
            placed.Value = collection.Count(x => x.Id != Constants.BarricadeId);
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

            int current = queue.Count + 1;
            int count = current > limit ? current - limit : 0;
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
                var character = _userRepo.characters.FirstOrDefault(x => x.id == data.id);
                if (character == null)
                    continue;
                
                var position = new Vector2(data.x, data.y);
                tasks.Add(CreateCharacter(character, position));
            }

            await UniTask.WhenAll(tasks);
        }

        private async UniTask CreateCharacter(UnitInfo info, Vector2 position)
        {
            UnitBehaviour unit;
            if (info.id != Constants.BarricadeId)
                unit = await _unitFactory.Create(info, position);
            else
                unit = await _unitFactory.CreateBarricade(info, position);

            if (unit == null)
                return;

            unit.Core.movement.Default = position;
            unit.Core.onRest.Value = true;
            
            _gameRepo.characters.Add(unit);
        }
        
        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}