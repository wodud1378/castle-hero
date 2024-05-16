using System;
using RGLabs.Common.Behaviours;
using RGLabs.InGame.System;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

namespace RGLabs.InGame.UI
{
    public class UIDamage : PoolItemBase
    {
        public enum Position
        {
            Top,
            Direction
        }

        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _label;
        [SerializeField] private Position _position;

        private RectTransform _rectTransform;
        private Func<IModifier, Vector2> _calcPosition;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;

            switch (_position)
            {
                case Position.Direction:
                    _calcPosition = CalculatePositionOnDirection;
                    break;
                default:
                    _calcPosition = CalculatePositionOnTop;
                    break;
            }
        }

        public void Show(IModifier modify)
        {
            _label.text = ((int)modify.Amount).ToString();
            _rectTransform.position = _calcPosition.Invoke(modify);
        }

        private Vector2 CalculatePositionOnDirection(IModifier modifier)
        {
            var from = modifier.From;
            if (!from.IsValid())
                return CalculatePositionOnTop(modifier);
            
            var closest = from.Collider.ClosestPoint(modifier.To.position);
            return closest * Random.Range(0.9f, 1.1f);
        }

        private Vector2 CalculatePositionOnTop(IModifier modify)
        {
            var bounds = modify.To.Collider.bounds;
            return new Vector2(bounds.center.x, bounds.max.y);
        }
    }
}