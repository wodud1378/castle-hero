using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_Upgrade.prefab")]
    public class PopupRateUp : PopupGrowth
    {
        [FormerlySerializedAs("_name")]
        [SerializeField] private TMP_Text name;
        [FormerlySerializedAs("_stars")]
        [SerializeField] private Image[] stars;
        [FormerlySerializedAs("_unitSlot")]
        [SerializeField] private UICharacterSlot unitSlot;
        [FormerlySerializedAs("_soulSlot")]
        [SerializeField] private UIItemSlot soulSlot;
        [FormerlySerializedAs("_requireSoul")]
        [SerializeField] private TMP_Text requireSoul;
        [FormerlySerializedAs("_requireGold")]
        [SerializeField] private TMP_Text requireGold;

        [FormerlySerializedAs("_enableOnMaxRate")]
        [SerializeField] private List<GameObject> enableOnMaxRate;
        [FormerlySerializedAs("_disableOnMaxRate")]
        [SerializeField] private List<GameObject> disableOnMaxRate;

        private IDBProvider _db;

        protected override GrowthAction Action => GrowthAction.Rate;

        protected override void OnAwake()
        {
            base.OnAwake();

            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();

            _userRepo.Inventory
                .WhenUpdate(_ => _unit.Value = _unit.Value)
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters) => base.Open(parameters);

        private void DefaultSetting(UnitInfo unit, UnitEntity entity)
        {
            name.text = entity.name;
            int rate = unit.rate;
            for (int i = 0; i < stars.Length; ++i)
            {
                var star = stars[i];
                star.DOKill();
                var c = star.color; c.a = 1f; star.color = c;
                star.gameObject.SetActive((i + 1) <= rate);
            }

            bool isMaxRate = rate == _db.Rates.MaxRate;
            enableOnMaxRate.ForEach(x => x.gameObject.SetActive(isMaxRate));
            disableOnMaxRate.ForEach(x => x.gameObject.SetActive(!isMaxRate));

            unitSlot.Init(unit, entity);
        }

        private void SettingIfNotMaxRate(UnitInfo unit, UnitEntity unitEntity, UnitRateEntity rateEntity)
        {
            int soulItemId = unitEntity.soulItemId;
            var item = _userRepo.Inventory.Items.FirstOrDefault(x => x.ItemId == soulItemId) ?? new Item
            {
                ItemId = soulItemId,
            };

            int nextRateIndex = unit.rate;
            if (nextRateIndex.IsValidIndex(stars))
            {
                var star = stars[nextRateIndex];
                star.gameObject.SetActive(true);
                // DOTween Free의 Image.DOFade 는 Modules 의존. Color alpha 를 DOTween.To 코어 API 로 보간.
                DOTween.To(() => star.color.a, v => { var c = star.color; c.a = v; star.color = c; }, 1f, 0.5f)
                    .From(0f)
                    .SetLoops(-1, LoopType.Yoyo);
            }

            string text = $"{item.Quantity}/{rateEntity.soul}";
            requireSoul.text = item.Quantity >= rateEntity.soul
                ? text.WithColor(Color.white)
                : text.WithNegativeColor();

            int requireGoldVal = rateEntity.gold;
            var hasEnoughGold = requireGoldVal <= _userRepo.Currency.Gold.Value;
            var goldText = requireGoldVal.CurrencyText();
            requireGold.text = hasEnoughGold
                ? goldText.WithColor(Color.white)
                : goldText.WithNegativeColor();

            _goldSlot.QuantityLabelColor = hasEnoughGold
                ? Color.white
                : StringHelper.NegativeColor;

            _confirm.interactable = hasEnoughGold;
            soulSlot.Init(item);
        }

        protected override void OnUnitChanged(UnitInfo unit)
        {
            if (!_db.Units.TryFind(unit.id, out var unitEntity) ||
                !_db.Rates.TryFind(unit.rate, out var rateEntity))
            {
                var exception = new Exception("데이터를 불러오지 못했습니다.");
                Debug.LogError(exception);
                return;
            }

            DefaultSetting(unit, unitEntity);
            if (unit.rate == _db.Rates.MaxRate)
                return;

            SettingIfNotMaxRate(unit, unitEntity, rateEntity);
        }

        protected override (int id, int quantity) ConsumeItem()
        {
            if (soulSlot.Item == null || !_db.Rates.TryFind(_unit.Value.rate, out var rateEntity))
                return default;

            return (soulSlot.Item.ItemId, rateEntity.soul);
        }
    }
}
