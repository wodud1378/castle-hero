using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_EquipmentCompare.prefab")]
    public class PopupCompareEquipment : PopupBase
    {
        [SerializeField] private UIEquipmentSlot _left;
        [SerializeField] private UIEquipmentSlot _right;
        [SerializeField] private UIStatusText[] _statusTexts;
        [SerializeField] private Button _equip;

        private UnitInfo _unit;
        private EquipItem _leftItem;
        private EquipItem _rightItem;

        protected override void OnAwake()
        {
            base.OnAwake();

            this.SubscribeButton(_equip, OnEquip, Storage.soundPath.equipItem);
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            if (parameters[1] is not EquipItem right)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            _rightItem = right;

            switch (parameters[0])
            {
                case UnitInfo unit:
                {
                    _unit = unit;
                    _leftItem = unit.equipments != null
                        ? Storage.userRepository.EquipItems(unit.equipments).FirstOrDefault(x => x.slot == right.slot)
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

            _left.Dispose();
            _right.Dispose();
        }

        private async void OnEquip()
        {
            if (_unit == null)
                return;

            var result = await NetworkService.Character.Equip(_unit.id, _rightItem.Guid);
            if (!result.IsSuccess)
            {
                Context.popups.Open<PopupCommon>(result.error);
                return;
            }

            Close();
        }

        private void UpdateUI()
        {
            if (_leftItem != null)
                _left.Init(_leftItem).Forget();

            _right.Init(_rightItem).Forget();

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

            foreach (var label in _statusTexts)
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