using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Network.Model;
using UnityEngine;

namespace RGLabs.Lobby.UI.Popup
{
    public class PopupCompareEquipment : PopupBase
    {
        [SerializeField] private UIStatusText[] _statusTexts;

        public override UniTask OpenTask(params object[] parameters)
        {
            if (parameters == null || parameters.Length < 2)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            if (parameters[0] is not EquipItem left ||
                parameters[1] is not EquipItem right)
            {
                var exception = new Exception("파라미터가 잘못되었습니다.");
                return UniTask.FromException(exception);
            }

            UpdateUI(left, right);
            return UniTask.CompletedTask;
        }

        private void UpdateUI(EquipItem leftItem, EquipItem rightItem)
        {
            // 타입, 값을 묶은 튜플 배열 l, r
            var l = leftItem.stats.Zip(leftItem.values, (type, value) => (type, value)).ToArray();
            var r = rightItem.stats.Zip(rightItem.values, (type, value) => (type, value)).ToArray();
            
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