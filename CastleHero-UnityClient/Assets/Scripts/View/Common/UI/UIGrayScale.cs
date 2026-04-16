using System.Collections.Generic;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace CastleHero.View.Common.UI
{
    public class UIGrayScale : MonoBehaviour
    {
        [FormerlySerializedAs("_material")]
        [SerializeField] private Material material;

        public Graphic[] targets;
        public new readonly BoolReactiveProperty enabled = new();

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

                target.material = material;
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

#if UNITY_EDITOR
        [FormerlySerializedAs("_preview")]
        [SerializeField] private bool preview;

        private void OnValidate()
        {
            if (material == null)
            {
                material = AssetDatabase.LoadAssetAtPath<Material>(
                    "Assets/BundleResources/01.Global/Materials/GrayScale.mat");
            }

            if(preview)
                Apply();
            else
                Release();
        }
#endif
    }
}
