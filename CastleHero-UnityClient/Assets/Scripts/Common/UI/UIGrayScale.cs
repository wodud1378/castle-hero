using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Common.UI
{
    public class UIGrayScale : MonoBehaviour
    {
        [SerializeField] private Material _material;

        public Graphic[] targets;
        public readonly BoolReactiveProperty enabled = new();

        private readonly Dictionary<Graphic, Material> _cache = new();

        private bool _isApplied;

        private void Awake()
        {
            enabled
                .Subscribe(x =>
                {
                    if (x) Apply();
                    else Release();
                })
                .AddTo(this);
        }

        private void Apply()
        {
            if (_isApplied)
                return;

            _cache.Clear();
            foreach (var target in targets)
            {
                if (!_cache.ContainsKey(target))
                {
                    _cache[target] = target.material;
                }

                target.material = _material;
            }

            _isApplied = true;
        }

        private void Release()
        {
            if (!_isApplied)
                return;

            foreach (var kvp in _cache)
            {
                kvp.Key.material = kvp.Value;
            }

            _cache.Clear();

            _isApplied = false;
        }
    }
}