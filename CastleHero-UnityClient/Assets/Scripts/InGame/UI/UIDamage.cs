using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.Behaviours;
using RGLabs.InGame.System;
using TMPro;
using UnityEngine;

namespace RGLabs.InGame.UI
{
    public class UIDamage : PoolItemBase
    {
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Color _normalDamage;
        [SerializeField] private Color _criticalDamage;

        [SerializeField] private float _yPosAmount;
        [SerializeField] private float _punchAmount;
        [SerializeField] private float _duration;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public async void Show(AtkResult atk)
        {
            _label.text = ((int)atk.amount).ToString();
            _label.color = atk.isCritical ? _criticalDamage : _normalDamage;

            var bounds = atk.to.Collider.bounds;
            float x = bounds.center.x;
            float y = bounds.max.y;
            _rectTransform.position = new Vector3(x, y, 0);

            var yTarget = _rectTransform.localPosition.y + _yPosAmount;
            _rectTransform.DOLocalMoveY(yTarget, _duration);
            _rectTransform.DOPunchScale(Vector3.one * _punchAmount, _duration);

            await UniTask.Delay(TimeSpan.FromSeconds(_duration));

            DestroySelf();
        }
    }
}