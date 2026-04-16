using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace CastleHero.View.Common.UI
{
    public abstract class UIListAdapter<TSlot, TData> : MonoBehaviour, IDisposable
        where TSlot : UISlot
    {
        public event Action<TSlot> OnSlotClickEvent;

        [SerializeField] protected RectTransform itemRoot;
        [FormerlySerializedAs("_itemPrefab")]
        [SerializeField] private AssetReference itemPrefab;

        public readonly List<TSlot> items = new();

        private GameObject _cachedPrefab;

        public virtual async UniTask Init(IEnumerable<TData> collection)
        {
            Clear();

            // 최초 호출 시만 1프레임 블로킹, 이후 캐시에서 즉시 사용
            if (_cachedPrefab == null && itemPrefab != null)
            {
                var handle = itemPrefab.LoadAssetAsync<GameObject>();
                _cachedPrefab = handle.WaitForCompletion();
            }

            foreach (var data in collection)
            {
                await Add(data);
            }
        }

        private void OnDestroy()
        {
            if (_cachedPrefab != null)
            {
                Addressables.Release(_cachedPrefab);
                _cachedPrefab = null;
            }
        }

        public TSlot GetItem(PointerEventData eventData)
        {
            if (!RectTransformUtility
                    .RectangleContainsScreenPoint(itemRoot, eventData.position, eventData.pressEventCamera))
                return null;

            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(itemRoot, eventData.position, eventData.pressEventCamera, out var worldPos);

            TSlot selected = null;
            float closest = float.MaxValue;
            foreach (TSlot item in items)
            {
                RectTransform childRectTransform = item.GetComponent<RectTransform>();
                if (childRectTransform != null)
                {
                    Vector2 position = childRectTransform.position;

                    float distance = Vector2.Distance(worldPos, position);
                    if (distance < closest)
                    {
                        closest = distance;
                        selected = item;
                    }
                }
            }

            return selected;
        }

        public void Dispose() => Clear();

        public void Clear()
        {
            items.ForEach(x =>
            {
                if (x.gameObject == null)
                    return;

                x.Dispose();
                UnityEngine.Object.Destroy(x.gameObject);
            });

            items.Clear();
        }

        protected abstract UniTask SetItem(TSlot slot, TData data);

        protected virtual TSlot ProvideSlot(TData data)
        {
            if (_cachedPrefab == null)
                return null;

            var obj = UnityEngine.Object.Instantiate(_cachedPrefab, itemRoot);
            return obj.TryGetComponent(out TSlot slot) ? slot : null;
        }

        private async UniTask Add(TData data)
        {
            var item = ProvideSlot(data);
            if (item == null)
                return;

            item.OnClick += (x) =>
            {
                if (x is not TSlot slot)
                    return;

                OnClick(slot);
            };

            items.Add(item);

            await SetItem(item, data);
        }

        private void OnClick(TSlot slot) => OnSlotClickEvent?.Invoke(slot);
    }
}
