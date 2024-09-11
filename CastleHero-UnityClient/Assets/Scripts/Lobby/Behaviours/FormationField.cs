using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using PolyNav;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Data.Repositories;
using RGLabs.Network;
using RGLabs.Network.Service;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using RGLabs.Network.Shared;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.Behaviours
{
    public class FormationField : MonoBehaviour
    {
        [field: SerializeField] public float Radius { get; private set; }

        [SerializeField] private PolyNavMap _map;
        [SerializeField] private int _defaultCastleId;
        [SerializeField] private float _autoPlacementRadius;

        private readonly Collider2D[] _buffer = new Collider2D[Constants.BufferSize];

        private CastleFactory _castleFactory;
        private UnitFactory _unitFactory;

        private UserRepository _userRepo;
        private InGameRepository _gameRepo;
        private Formation _formation;

        public readonly ReactiveProperty<int> capacity = new();
        public readonly ReactiveProperty<int> placed = new();

        private int _barricadeCountMax;

        public async UniTask Init()
        {
            _userRepo = Storage.userRepository;
            _gameRepo = Storage.inGameRepository;
            _castleFactory = Context.castleFactory;
            _unitFactory = Context.unitFactory;

            _formation = _userRepo.formation;

            _gameRepo.characters
                .ChangeAsObservable()
                .Subscribe(OnFieldCharacterCollectionChanged)
                .AddTo(this);

            _userRepo.gameRecord.castleLv
                .Subscribe(OnCastleLevelChanged)
                .AddTo(this);

            _userRepo.characters
                .WhenUpdate(UpdateUnits)
                .AddTo(this);

            _userRepo.inventory
                .WhenUpdate(UpdateUnitsWhereHasEquipments)
                .AddTo(this);

            _userRepo.entrance
                .Subscribe(ReloadCastle)
                .AddTo(this);

            if (Storage.db.castles.TryFind(_userRepo.gameRecord.castleLv.Value, out var entity))
            {
                capacity.Value = entity.maxCharacter;
                _barricadeCountMax = entity.barricadeCount;
            }

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

        private void UpdateUnits(ReactiveCollection<UnitInfo> units)
        {
            foreach (var unit in units)
            {
                var inField = _gameRepo.characters.FirstOrDefault(x => x.Id == unit.id);
                if (inField != null &&
                    Storage.db.units.TryFind(unit.id, out var entity) &&
                    Storage.db.balances.TryFind(unit.id, out var balance))
                {
                    inField.Core.Update(unit, entity, balance);
                }
            }
        }

        private void UpdateUnitsWhereHasEquipments(ReactiveCollection<IItem> items)
        {
            var ids = items.OfType<EquipItem>()
                .Select(x => x.character)
                .Where(x => x != 0)
                .Distinct();

            foreach (var id in ids)
            {
                var unit = _userRepo.characters.units.FirstOrDefault(x => x.id == id);
                var inField = _gameRepo.characters.FirstOrDefault(x => x.Id == id);

                if (unit != null &&
                    inField != null &&
                    Storage.db.units.TryFind(unit.id, out var entity) &&
                    Storage.db.balances.TryFind(unit.id, out var balance))
                {
                    inField.Core.Update(unit, entity, balance);
                }
            }
        }

        private void ReloadCastle(GameEntrance entrance)
        {
            string prefab;
            if (entrance.type == GameType.Dungeon)
            {
                prefab = Storage.db.dungeons.TryFind(entrance.id, out var dungeonEntity)
                    ? dungeonEntity.castlePrefab
                    : string.Empty;
            }
            else
            {
                prefab = CastleFactory.DEFAULT_CASTLE_PREFAB;
            }
            
            LoadCastle(prefab).Forget();
        }

        private void OnCastleLevelChanged(int lv)
        {
            if (Storage.db.castles.TryFind(lv, out var entity))
            {
                capacity.Value = entity.maxCharacter;
                _barricadeCountMax = entity.barricadeCount;

                var castle = _gameRepo.castle.Value;
                if (castle != null)
                {
                    var unitInfo = new UnitInfo { id = 1, lv = lv, };
                    castle.Init(unitInfo, entity.ToUnitEntity(), default);
                }
            }
        }

        public async void AutoPlacement()
        {
            Clear();

            var characters = _userRepo.characters.units
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

            _formation.Set(_gameRepo.characters);

            Save();
        }

        public void Clear()
        {
            foreach (var unit in _gameRepo.characters)
            {
                unit.DestroySelf();
            }

            _gameRepo.characters.Clear();
            _formation.Set(_gameRepo.characters);
        }

        public void Remove(UnitBehaviour unit)
        {
            if (unit == null)
                return;

            if (unit == _gameRepo.castle.Value)
                return;

            if (unit.Id == Constants.BarricadeId)
                _map.RemoveObstacle(unit);

            var characters = _gameRepo.characters;
            characters.Remove(unit);
            unit.DestroySelf();

            _formation.Set(_gameRepo.characters);

            Save();
        }

        public bool TryRegister(UnitBehaviour unit, int layer, bool isExist)
        {
            if (unit == null)
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
                {
                    Context.popups.Open<PopupCommon>(Storage.localize.Get(593));
                    return false;
                }

                RemoveIfLimited(unit);
                characters.Add(unit);
            }

            _formation.Set(characters);

            if (isBarricade)
                _map.AddObstacle(unit);

            Save();

            Context.sounds.PlaySfx(Storage.soundPath.modifyFormation);

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

        private CancellationTokenSource _castleCreationCancel;

        private async UniTask LoadCastle(string prefab)
        {
            _castleCreationCancel?.Cancel();
            _castleCreationCancel = new();

            var legacy = _gameRepo.castle.Value;
            if (legacy != null)
            {
                _gameRepo.castle.Value = null;
                legacy.DestroySelf();
            }

            if (string.IsNullOrEmpty(prefab))
                return;
            
            int lv = _userRepo.gameRecord.castleLv.Value;
            var info = new UnitInfo { id = 1, lv = lv, };
            var task = await _castleFactory
                .Create(prefab, info, Vector2.zero)
                .AttachExternalCancellation(_castleCreationCancel.Token)
                .SuppressCancellationThrow();

            if (task.IsCanceled)
                return;
            
            _gameRepo.castle.Value = task.Result;
        }

        private async UniTask LoadSavedUnits()
        {
            var tasks = new List<UniTask>();
            foreach (var data in _userRepo.formation.fieldUnits)
            {
                var character = _userRepo.characters.units.FirstOrDefault(x => x.id == data.id);
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

        private void Save()
        {
            NetworkService.User.SaveFormation(
                new FormationDto { fieldUnits = _userRepo.formation.fieldUnits.ToList() });
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.DrawWireSphere(transform.position, Radius);
        }
    }
}