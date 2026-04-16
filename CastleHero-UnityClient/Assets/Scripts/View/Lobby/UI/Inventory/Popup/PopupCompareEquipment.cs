using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.View.Lobby.UI;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using CastleHero.Common.Pattern;

using CastleHero.Data.Repositories;
using CastleHero.Common.Sound;
namespace CastleHero.View.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_EquipmentCompare.prefab")]
    public class PopupCompareEquipment : PopupBase, ISelect<bool>
    {
        [FormerlySerializedAs("_left")]
        [SerializeField] private UIEquipmentSlot left;
        [FormerlySerializedAs("_right")]
        [SerializeField] private UIEquipmentSlot right;
        [FormerlySerializedAs("_statusTexts")]
        [SerializeField] private UIStatusText[] statusTexts;
        [FormerlySerializedAs("_equip")]
        [SerializeField] private Button equip;

        private UnitInfo _unit;
        private EquipItem _leftItem;
        private EquipItem _rightItem;

        public UniTask<bool> SelectTask => _ctSource.Task;

        private UniTaskCompletionSource<bool> _ctSource;

        public void BeginSelect(bool _) => _ctSource = new();

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(equip, OnEquip, ServiceLocator.Get<SoundPath>().equipItem);
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            if (parameters[1] is not EquipItem rightItem)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            _rightItem = rightItem;

            switch (parameters[0])
            {
                case UnitInfo unit:
                {
                    _unit = unit;
                    _leftItem = unit.equipments != null
                        ? ServiceLocator.Get<IUserRepository>().EquipItems(unit.equipments).FirstOrDefault(x => x.slot == rightItem.slot)
                        : null;
                    break;
                }
                case EquipItem e:
                    _leftItem = e;
                    break;
            }

            UpdateUI();
            return UniTask.CompletedTask;
        }

        protected override void OnClose()
        {
            base.OnClose();

            left.Dispose();
            right.Dispose();

            if (_ctSource != null)
            {
                _ctSource.TrySetResult(false);
                _ctSource = null;
            }
        }

        private async UniTask OnEquip()
        {
            if (_unit == null)
                return;

            if (_ctSource != null)
            {
                _ctSource.TrySetResult(true);
                _ctSource = null;
            }
            else
            {
                var result = await ServiceLocator.Get<INetworkServiceProvider>().Character.Equip(_unit.id, _rightItem.Guid);
                if (!result.IsSuccess)
                {
                    ServiceLocator.Get<IPopupManager>().Open<PopupCommon>(result.error);
                    return;
                }
            }

            Close();
        }

        private void UpdateUI()
        {
            if (_leftItem != null)
                left.Init(_leftItem).Forget();

            right.Init(_rightItem).Forget();

            UpdateText(_leftItem, _rightItem);
        }

        private void UpdateText(EquipItem leftItem, EquipItem rightItem)
        {
            // 타입, 값을 묶은 튜플 배열 l, r
            var l = leftItem != null
                ? leftItem.sub
                    .Append(leftItem.main)
                    .Select(x => (x.type, x.value))
                    .ToArray()
                : Array.Empty<(int type, float value)>();

            var r = rightItem.sub
                .Append(rightItem.main)
                .Select(x => (x.type, x.value))
                .ToArray();

            // 교집합
            var intersection = l
                .SelectMany(left => r.Where(right => right.type == left.type),
                    (left, right) => (left, right))
                .ToArray();

            // 여집합
            var exceptL = l.Except(intersection.Select(x => x.left)).ToArray();
            var exceptR = r.Except(intersection.Select(x => x.right)).ToArray();

            foreach (var label in statusTexts)
            {
                int labelType = (int)label.type;
                var elementI = intersection.FirstOrDefault(x => x.left.type == labelType);
                if (elementI != default)
                {
                    float newVal = elementI.right.value;
                    label.SetText(newVal, newVal - elementI.left.value);
                    label.gameObject.SetActive(true);
                    continue;
                }

                var elementL = exceptL.FirstOrDefault(x => x.type == labelType);
                if (elementL != default)
                {
                    label.SetText(0, -elementL.value);
                    label.gameObject.SetActive(true);
                    continue;
                }

                var elementR = exceptR.FirstOrDefault(x => x.type == labelType);
                if (elementR != default)
                {
                    label.SetText(elementR.value, elementR.value);
                    label.gameObject.SetActive(true);
                    continue;
                }

                label.gameObject.SetActive(false);
            }
        }
    }
}
