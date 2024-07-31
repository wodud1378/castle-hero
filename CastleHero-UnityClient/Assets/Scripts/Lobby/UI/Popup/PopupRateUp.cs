using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Network.Service.Character;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Upgrade.prefab")]
    public class PopupRateUp : PopupBase, IGrowthTask
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image[] _stars;
        [SerializeField] private UICharacterSlot _unitSlot;
        [SerializeField] private UIItemSlot _soulSlot;
        [SerializeField] private TMP_Text _requireSoul;
        [SerializeField] private TMP_Text _requireGold;

        [SerializeField] private UIItemSlot _goldSlot;
        [SerializeField] private Button _confirm;

        public UniTask<GrowthResult> GrowthTask => _completionSource.Task;
        
        private UniTaskCompletionSource<GrowthResult> _completionSource;
        private GrowthResult _result;
        
        private readonly ReactiveProperty<UnitInfo> _unit = new();
        private readonly CharacterService _service = new ();
        
        private UniTask _updateTask;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_confirm, () => Confirm().Forget());

            Storage.userRepository.gold
                .Subscribe(UpdateGoldSlot)
                .AddTo(this);
            
            _unit
                .Subscribe(UpdateUI)
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters.Length == 0 || parameters[0] is not UnitInfo unitInfo)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            _completionSource = new();
            _unit.Value = unitInfo;
            return _updateTask;
        }

        protected override void OnClose()
        {
            base.OnClose();

            _completionSource.TrySetResult(_result);
        }

        private void UpdateGoldSlot(int gold)
        {
            _goldSlot.Init(new Item
            {
                ItemId = Constants.GoldId,
                Quantity = gold
            });
        }

        private void UpdateUI(UnitInfo unitInfo)
        {
            if (unitInfo == null)
                return;

            int rate = unitInfo.rate;
            if (!Storage.db.rates.TryFind(rate, out var rateEntity))
            {
                var exception = new Exception("데이터를 불러오지 못했습니다.");
                Debug.LogError(exception);
                return;
            }

            if (!Storage.db.units.TryFind(unitInfo.id, out var unitEntity))
            {
                var exception = new Exception("데이터를 불러오지 못했습니다.");
                Debug.LogError(exception);
                return;
            }

            _name.text = unitEntity.name;

            int soulItemId = unitEntity.soulItemId;
            var item = Storage.userRepository.items.FirstOrDefault(x => x.ItemId == soulItemId) ?? new Item
            {
                ItemId = soulItemId,
            };

            for (int i = 0; i < _stars.Length; ++i)
            {
                var star = _stars[i];
                star.DOKill();
                star.gameObject.SetActive((i + 1) <= rate);
            }

            int nextRateIndex = rate + 2;
            if (nextRateIndex.IsValidIndex(_stars))
            {
                var star = _stars[nextRateIndex];
                star.gameObject.SetActive(true);
                star.DOFade(1f, 0.5f)
                    .From(0f)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            string text = $"{item.Quantity}/{rateEntity.soul}";
            _requireSoul.text = item.Quantity >= rateEntity.soul
                ? text.WithColor(Color.white)
                : text.WithNegativeColor();

            int requireGold = rateEntity.gold;
            bool isGoldEnough = requireGold <= Storage.userRepository.gold.Value;
            var goldText = requireGold.CurrencyText();
            _requireGold.text = isGoldEnough
                ? goldText.WithColor(Color.white)
                : goldText.WithNegativeColor();

            _goldSlot.QuantityLabelColor = isGoldEnough
                ? Color.white
                : StringHelper.NegativeColor;

            var unitTask = _unitSlot.Init(unitInfo, unitEntity);
            var soulTask = _soulSlot.Init(item);

            _updateTask = UniTask.WhenAll(unitTask, soulTask);
        }

        private async UniTaskVoid Confirm()
        {
            if (_soulSlot.Item is not Item item)
                return;

            if (!Storage.db.rates.TryFind(_unit.Value.rate, out var rateEntity))
                return;

            var result = await _service.Upgrade(_unit.Value.id, item.ItemId, rateEntity.soul);
            var unit = result.transition.unit;
            var repository = Storage.userRepository;
            repository.Update(result.leftCurrency);
            repository.Update(result.leftItem);
            repository.Update(unit);
            
            _unit.Value = unit;
            _result = result;
        }
    }
}