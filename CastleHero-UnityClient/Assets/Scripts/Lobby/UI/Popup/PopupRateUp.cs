using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
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
    [PrefabPath("Lobby/UI/Prefabs/Popup_Upgrade.prefab")]
    public class PopupRateUp : PopupGrowth
    {
        [SerializeField] private TMP_Text _name;
        [SerializeField] private Image[] _stars;
        [SerializeField] private UICharacterSlot _unitSlot;
        [SerializeField] private UIItemSlot _soulSlot;
        [SerializeField] private TMP_Text _requireSoul;
        [SerializeField] private TMP_Text _requireGold;

            
        private UniTask _updateTask;

        protected override CharacterService.GrowthAction Action => CharacterService.GrowthAction.Rate;

        protected override void OnAwake()
        {
            base.OnAwake();
            
            Storage.userRepository.inventory.items
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(_=> _unit.Value = _unit.Value)
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters)
        {
            return UniTask.WhenAll(base.Open(parameters), _updateTask);
        }
        
        protected override void OnUnitChanged(UnitInfo unit)
        {
            int rate = unit.rate;
            if (!Storage.db.rates.TryFind(rate, out var rateEntity))
            {
                var exception = new Exception("데이터를 불러오지 못했습니다.");
                Debug.LogError(exception);
                return;
            }

            if (!Storage.db.units.TryFind(unit.id, out var unitEntity))
            {
                var exception = new Exception("데이터를 불러오지 못했습니다.");
                Debug.LogError(exception);
                return;
            }

            _name.text = unitEntity.name;

            int soulItemId = unitEntity.soulItemId;
            var item = Storage.userRepository.inventory.items.FirstOrDefault(x => x.ItemId == soulItemId) ?? new Item
            {
                ItemId = soulItemId,
            };

            for (int i = 0; i < _stars.Length; ++i)
            {
                var star = _stars[i];
                star.DOKill();
                star.DOFade(1f, 0f);
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
            bool hasEnoughGold = requireGold <= Storage.userRepository.currency.gold.Value;
            var goldText = requireGold.CurrencyText();
            _requireGold.text = hasEnoughGold
                ? goldText.WithColor(Color.white)
                : goldText.WithNegativeColor();

            _goldSlot.QuantityLabelColor = hasEnoughGold
                ? Color.white
                : StringHelper.NegativeColor;

            _confirm.interactable = hasEnoughGold;

            var unitTask = _unitSlot.Init(unit, unitEntity);
            var soulTask = _soulSlot.Init(item);

            _updateTask = UniTask.WhenAll(unitTask, soulTask);
        }

        protected override (int id, int quantity) ConsumeItem()
        {
            if (_soulSlot.Item == null || !Storage.db.rates.TryFind(_unit.Value.rate, out var rateEntity))
                return default;

            return (_soulSlot.Item.ItemId, rateEntity.soul);
        }

        protected override void OnGrowthComplete(UnitGrowth growth)
        {
            if (growth.transition.unit.rate < Storage.db.rates.MaxRate)
                return;
            
            Close();
        }
    }
}