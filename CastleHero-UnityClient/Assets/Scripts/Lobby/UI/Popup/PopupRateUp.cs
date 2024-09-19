using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Upgrade.prefab")]
    public class PopupRateUp : PopupGrowth
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image[] _stars;
        [SerializeField] private UICharacterSlot _unitSlot;
        [SerializeField] private UIItemSlot _soulSlot;
        [SerializeField] private TMP_Text _requireSoul;
        [SerializeField] private TMP_Text _requireGold;

        [SerializeField] private List<GameObject> _enableOnMaxRate;
        [SerializeField] private List<GameObject> _disableOnMaxRate;
            
        private UniTask _updateTask;

        protected override CharacterService.GrowthAction Action => CharacterService.GrowthAction.Rate;

        protected override void OnAwake()
        {
            base.OnAwake();

            Storage.userRepository.inventory
                .WhenUpdate(_ => _unit.Value = _unit.Value)
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters) => UniTask.WhenAll(base.Open(parameters), _updateTask);

        private UniTask DefaultSettingTask(UnitInfo unit, UnitEntity entity)
        {
            _name.text = entity.name;
            int rate = unit.rate;
            for (int i = 0; i < _stars.Length; ++i)
            {
                var star = _stars[i];
                star.DOKill();
                star.DOFade(1f, 0f);
                star.gameObject.SetActive((i + 1) <= rate);
            }
            
            bool isMaxRate = rate == Storage.db.rates.MaxRate;
            _enableOnMaxRate.ForEach(x => x.gameObject.SetActive(isMaxRate));
            _disableOnMaxRate.ForEach(x => x.gameObject.SetActive(!isMaxRate));
            
            return _unitSlot.Init(unit, entity);
        }

        private UniTask SettingIfNotMaxRateTask(UnitInfo unit, UnitEntity unitEntity, UnitRateEntity rateEntity)
        {
            int soulItemId = unitEntity.soulItemId;
            var item = Storage.userRepository.inventory.items.FirstOrDefault(x => x.ItemId == soulItemId) ?? new Item
            {
                ItemId = soulItemId,
            };

            int nextRateIndex = unit.rate;
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
            var hasEnoughGold = requireGold <= Storage.userRepository.currency.gold.Value;
            var goldText = requireGold.CurrencyText();
            _requireGold.text = hasEnoughGold
                ? goldText.WithColor(Color.white)
                : goldText.WithNegativeColor();

            _goldSlot.QuantityLabelColor = hasEnoughGold
                ? Color.white
                : StringHelper.NegativeColor;

            _confirm.interactable = hasEnoughGold;
            return _soulSlot.Init(item);
        }
        
        protected override void OnUnitChanged(UnitInfo unit)
        {
            if (!Storage.db.units.TryFind(unit.id, out var unitEntity) ||
                !Storage.db.rates.TryFind(unit.rate, out var rateEntity))
            {
                var exception = new Exception("데이터를 불러오지 못했습니다.");
                Debug.LogError(exception);
                _updateTask = UniTask.CompletedTask;
                return;
            }

            var defaultTask = DefaultSettingTask(unit, unitEntity);
            if (unit.rate == Storage.db.rates.MaxRate)
            {
                _updateTask = defaultTask;
                return;
            }

            var additionalTask = SettingIfNotMaxRateTask(unit, unitEntity, rateEntity);
            _updateTask = UniTask.WhenAll(defaultTask, additionalTask);
        }

        protected override (int id, int quantity) ConsumeItem()
        {
            if (_soulSlot.Item == null || !Storage.db.rates.TryFind(_unit.Value.rate, out var rateEntity))
                return default;

            return (_soulSlot.Item.ItemId, rateEntity.soul);
        }
    }
}