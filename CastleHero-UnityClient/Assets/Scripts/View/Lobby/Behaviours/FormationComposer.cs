using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Localize;
using CastleHero.Common.Sound;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Factory;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Factory;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using CastleHero.View.Common;
using CastleHero.View.Common.UI.Popup;
using UniRx;
using UnityEngine;

namespace CastleHero.View.Lobby.Behaviours
{
    /// <summary>
    /// FormationDraft 의 소유자. 모든 배치 변경(등록/해제/자동배치/성 교체)을 수행하고 서버 저장을 트리거.
    /// FormationField 는 Scene 의존 파사드 역할만 수행하고, 이 클래스가 상태+로직을 담당한다.
    /// </summary>
    public class FormationComposer : IDisposable
    {
        private readonly CompositeDisposable _disposables = new();

        private readonly IFormationFieldArea _area;
        private readonly IUnitFactory _unitFactory;
        private readonly ICastleFactory _castleFactory;
        private readonly IUserRepository _userRepo;
        private readonly IDBProvider _db;
        private readonly IPopupManager _popups;
        private readonly ISoundManager _sounds;
        private readonly LocalizeText _localize;
        private readonly SoundPath _soundPath;
        private readonly INetworkServiceProvider _network;

        private readonly ReactiveProperty<int> _capacity = new();
        private readonly ReactiveProperty<int> _placed = new();
        private readonly Formation _formation;

        private int _barricadeCountMax;
        private CancellationTokenSource _castleCreationCancel;

        public FormationDraft Draft { get; }
        public IReadOnlyReactiveProperty<int> Capacity => _capacity;
        public IReadOnlyReactiveProperty<int> Placed => _placed;

        public FormationComposer(
            IFormationFieldArea area,
            IUnitFactory unitFactory,
            ICastleFactory castleFactory,
            IUserRepository userRepo,
            IDBProvider db,
            IPopupManager popups,
            ISoundManager sounds,
            LocalizeText localize,
            SoundPath soundPath,
            INetworkServiceProvider network)
        {
            _area = area;
            _unitFactory = unitFactory;
            _castleFactory = castleFactory;
            _userRepo = userRepo;
            _db = db;
            _popups = popups;
            _sounds = sounds;
            _localize = localize;
            _soundPath = soundPath;
            _network = network;

            Draft = new FormationDraft();
            _formation = _userRepo.Formation;

            Draft.Characters
                .ChangeAsObservable()
                .Subscribe(OnFieldCharacterCollectionChanged)
                .AddTo(_disposables);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _castleCreationCancel?.Cancel();
            _castleCreationCancel?.Dispose();
            Draft.Dispose();
        }

        public void ApplyCastleCapacity(int maxCharacter, int barricadeCount)
        {
            _capacity.Value = maxCharacter;
            _barricadeCountMax = barricadeCount;
        }

        public async UniTask LoadSavedUnits()
        {
            var tasks = new List<UniTask>();
            foreach (var data in _userRepo.Formation.FieldUnits)
            {
                var character = _userRepo.Characters.Units.FirstOrDefault(x => x.id == data.id);
                if (character == null)
                    continue;

                var position = new Vector2(data.x, data.y);
                tasks.Add(CreateCharacter(character, position));
            }

            await UniTask.WhenAll(tasks);
        }

        public async UniTask AutoPlacement()
        {
            Clear();

            var characters = _userRepo.Characters.Units
                .Where(x => x.id != Constants.BarricadeId)
                .ToList();

            int max = Mathf.Min(_capacity.Value, characters.Count);
            float anglePerOnce = 360f / max;
            int current = 0;
            var tasks = new List<UniTask>();
            using var itr = characters.GetEnumerator();
            while (itr.MoveNext() && current < max)
            {
                var pos = (anglePerOnce * current++ + 90f).ToVector() * _area.AutoPlacementRadius;
                tasks.Add(CreateCharacter(itr.Current, pos));
            }

            await UniTask.WhenAll(tasks);

            _formation.Set(Draft.Characters);
            Save();
        }

        public void Clear()
        {
            foreach (var unit in Draft.Characters)
                unit.DestroySelf();

            Draft.Characters.Clear();
            _formation.Set(Draft.Characters);
        }

        public void Remove(UnitBehaviour unit)
        {
            if (unit == null) return;
            if (unit == Draft.Castle.Value) return;

            if (unit.Id == Constants.BarricadeId)
                _area.RemoveObstacle(unit);

            Draft.Characters.Remove(unit);
            unit.DestroySelf();

            _formation.Set(Draft.Characters);
            Save();
        }

        public bool TryRegister(UnitBehaviour unit, int layer, bool isExist)
        {
            if (unit == null) return false;
            if (!_area.IsValid(unit.Collider, layer)) return false;

            bool isBarricade = unit.Id == Constants.BarricadeId;
            var characters = Draft.Characters;
            if (isExist)
            {
                int index = characters.IndexOf(unit);
                characters[index].Position = unit.Position;
            }
            else
            {
                if (!isBarricade && _placed.Value >= _capacity.Value)
                {
                    _popups.Open<PopupCommon>(_localize.Get(593));
                    return false;
                }

                RemoveIfLimited(unit);
                characters.Add(unit);
            }

            _formation.Set(characters);

            if (isBarricade)
                _area.AddObstacle(unit);

            Save();
            _sounds.PlaySfx(_soundPath.modifyFormation);
            return true;
        }

        public async UniTask LoadCastle(string prefab)
        {
            _castleCreationCancel?.Cancel();
            _castleCreationCancel = new CancellationTokenSource();

            var legacy = Draft.Castle.Value;
            if (legacy != null)
            {
                Draft.Castle.Value = null;
                legacy.DestroySelf();
            }

            if (string.IsNullOrEmpty(prefab))
                return;

            int lv = _userRepo.GameRecord.CastleLv.Value;
            var info = new UnitInfo { id = 1, lv = lv };
            var task = await _castleFactory
                .Create(prefab, info, Vector2.zero)
                .AttachExternalCancellation(_castleCreationCancel.Token)
                .SuppressCancellationThrow();

            if (task.IsCanceled)
                return;

            Draft.Castle.Value = task.Result;
        }

        public void ReapplyCastle(int lv, CastleEntity entity)
        {
            var castle = Draft.Castle.Value as UnitBehaviour;
            if (castle == null) return;

            var unitInfo = new UnitInfo { id = 1, lv = lv };
            castle.Init(unitInfo, entity.ToUnitEntity(), default);
        }

        public void ApplyBarricadesToMap()
        {
            var barricades = Draft.Characters
                .Where(x => x.Id == Constants.BarricadeId)
                .OfType<UnitBehaviour>()
                .ToArray();

            if (barricades.Length == 0)
                return;

            foreach (var barricade in barricades)
                _area.AddObstacle(barricade, false);

            _area.GenerateMap();
        }

        private async UniTask CreateCharacter(UnitInfo info, Vector2 position)
        {
            var created = info.id != Constants.BarricadeId
                ? await _unitFactory.Create(info, position)
                : await _unitFactory.CreateBarricade(info, position);

            if (created is not UnitBehaviour unit)
                return;

            unit.Core.Movement.Default = position;
            unit.Core.OnRest.Value = true;

            Draft.Characters.Add(unit);
        }

        private void RemoveIfLimited(UnitBehaviour unit)
        {
            int limit = unit.Type == UnitBehaviour.BehaviourType.Barricade ? _barricadeCountMax : 1;
            var queue = new Queue<IUnitBehaviour>();
            foreach (var character in Draft.Characters)
            {
                if (character.Id == unit.Id && character != unit)
                    queue.Enqueue(character);
            }

            int current = queue.Count + 1;
            int count = current > limit ? current - limit : 0;
            while (count > 0)
            {
                var character = queue.Dequeue();
                Draft.Characters.Remove(character);
                character.DestroySelf();
                --count;
            }
        }

        private void OnFieldCharacterCollectionChanged(ReactiveCollection<IUnitBehaviour> collection)
        {
            _placed.Value = collection.Count(x => x.Id != Constants.BarricadeId);
        }

        private void Save()
        {
            _network.User.SaveFormation(
                new FormationDto { fieldUnits = _userRepo.Formation.FieldUnits.ToList() });
        }
    }
}
