using System;
using System.Collections.Generic;
using System.Linq;
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
        
        public readonly List<TSlot> items = new();

        public virtual async UniTask Init(IEnumerable<TData> collection)
        {
            Clear();
            
            var tasks = new List<UniTask<(TSlot slot, int order)>>();
            int index = 0;
            foreach (var data in collection)
            {
                tasks.Add(Add(data, ++index));
            }

            var result = (await UniTask.WhenAll(tasks)).ToList();
            result.Sort((x, y)=> x.order.CompareTo(y.order));

            foreach (var tuple in result)
            {
                if (tuple.slot == null) 
                    continue;
                
                tuple.slot.transform.SetAsLastSibling();
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
                Addressables.ReleaseInstance(x.gameObject);
            });

            items.Clear();
        }

        protected abstract UniTask SetItem(TSlot slot, TData data);

        protected virtual async UniTask<TSlot> ProvideSlot(TData data) => await _itemPrefab.Instantiate<TSlot>(itemRoot);

        private async UniTask<(TSlot slot, int order)> Add(TData data, int order)
        {
            var item = await ProvideSlot(data);
            if (item == null)
                return default;

            item.OnClick += (x) =>
            {
                if (x is not TSlot slot)
                    return;

                OnClick(slot);
            };
            
            items.Add(item);
            
            await SetItem(item, data);
            
            return (item, order);
        }

        private void OnClick(TSlot slot) => OnSlotClickEvent?.Invoke(slot);
    }
}