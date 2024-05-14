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
        [Serializable]
        public struct Set
        {
            public DamageType type;
            public bool isCritical;
            public Color mainColor;
            public Color outlineColor;
        }

        [SerializeField] private Set[] _sets;
        
        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _label;

        private RectTransform _rectTransform;

        private void Awake()
        {
            _rectTransform = transform as RectTransform;
        }

        public void Show(AtkResult atk)
        {
            _rectTransform.position = CalculatePosition(atk);
        }

        private Vector2 CalculatePosition(AtkResult atk)
        {
            if (atk.type == DamageType.Normal && atk.from.IsValid())
            {
                var closest = atk.from.Collider.ClosestPoint(atk.to.position);
                return closest * Random.Range(0.9f, 1.1f);
            }

            var bounds = atk.to.Collider.bounds;
            return new Vector2(bounds.center.x, bounds.max.y);
        }
    }
}