using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Network.Service.Character;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Upgrade.prefab")]
    public class PopupRateUp : PopupBase
    {
        [SerializeField] private Image[] _stars;
        [SerializeField] private UICharacterSlot _unitSlot;
        [SerializeField] private UIItemSlot _soulSlot;
        [SerializeField] private TMP_Text _requireSoul;
        [SerializeField] private TMP_Text _requireGold;

        [SerializeField] private Button _confirm;

        private readonly ReactiveProperty<UnitInfo> _unit = new();
        private readonly ICharacterService _service = new LocalCharacterService();

        private UniTask _updateTask;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_confirm, () => Confirm().Forget());

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

            _unit.Value = unitInfo;
            return _updateTask;
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

            int soulItemId = unitEntity.soulItemId;
            var item = Storage.userRepository.items.FirstOrDefault(x => x.ItemId == soulItemId) ?? new ConsumableItem
            {
                ItemId = soulItemId,
                consumeOption = (int)ConsumeOption.Soul
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

            _requireGold.text = rateEntity.gold.CurrencyText();

            var unitTask = _unitSlot.Init(unitInfo, unitEntity);
            var soulTask = _soulSlot.Init(item);

            _updateTask = UniTask.WhenAll(unitTask, soulTask);
        }

        private async UniTaskVoid Confirm()
        {
            if (_soulSlot.Item is not ConsumableItem item)
                return;

            if (!Storage.db.rates.TryFind(_unit.Value.rate, out var rateEntity))
                return;

            var result = await _service.Upgrade(_unit.Value, item, rateEntity.soul);
            _unit.Value = result.Info;
            _soulSlot.Init(result.ItemResult);
        }
    }
}