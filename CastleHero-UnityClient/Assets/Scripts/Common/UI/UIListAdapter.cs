using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace RGLabs.Common.UI
{
    public abstract class UIListAdapter<TSlot, TData> : MonoBehaviour, IDisposable
        where TSlot : UISlot
    {
        public event Action<TSlot> OnSlotClickEvent;
        
        [SerializeField] protected RectTransform itemRoot;

        [SerializeField] private AssetReference _itemPrefab;
        
        protected readonly List<TSlot> _items = new();

        public virtual async UniTask Init(IEnumerable<TData> collection, Action<UISlot> onClick = null)
        {
            Clear();

            var tasks = new List<UniTask>();
            foreach (var data in collection)
            {
                tasks.Add(Add(data));
            }

            await UniTask.WhenAll(tasks);
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
            foreach (TSlot item in _items)
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
            _items.ForEach(x =>
            {
                x.Dispose();
                Addressables.ReleaseInstance(x.gameObject);
            });

            _items.Clear();
        }

        protected abstract UniTask SetItem(TSlot slot, TData data);

        private async UniTask<TSlot> Add(TData data)
        {
            var item = await _itemPrefab.Instantiate<TSlot>(itemRoot);
            if (item == null)
                return null;

            item.OnClick += (x) =>
            {
                if (x is not TSlot slot)
                    return;

                OnClick(slot);
            };
            
            _items.Add(item);
            await SetItem(item, data);
            
            return item;
        }

        private void OnClick(TSlot slot) => OnSlotClickEvent?.Invoke(slot);
    }
}