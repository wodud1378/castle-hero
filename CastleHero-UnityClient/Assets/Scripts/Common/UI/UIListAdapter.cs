using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;

namespace RGLabs.Common.UI
{
    public abstract class UIListAdapter<TItem, TData> : MonoBehaviour, IDisposable
        where TItem : UIItemSlot
    {
        [SerializeField] protected RectTransform itemRoot;

        [SerializeField] private AssetReference _itemPrefab;
        
        private readonly List<TItem> _items = new();

        private CancellationTokenSource _ctSource;
        private CancellationToken Ct => _ctSource?.Token ?? default;
        
        public async UniTask Init(IEnumerable<TData> array, Action<UIItemSlot> onClick = null)
        {
            _ctSource?.Cancel();
            _ctSource = new CancellationTokenSource();

            Clear();

            var tasks = new List<UniTask>();
            int order = 0;
            foreach (var data in array)
            {
                tasks.Add(Add(data, order++, onClick));
            }

            await tasks.WhenAll(Ct);
        }

        public TItem GetItem(Vector2 position)
        {
            var corners = new Vector3[4];
            
            itemRoot.GetWorldCorners(corners);

            var rootPos = itemRoot.position;
            var width = (corners[2] - corners[1]).x;
            var height = (corners[1] - corners[0]).y;
            rootPos.y -= height;
            var rect = new Rect(rootPos.x, rootPos.y, width, height);

            if (!rect.Contains(position))
                return null;

            TItem selected = null;
            float lastDistance = float.MaxValue;
            foreach (var item in _items)
            {
                float distance = Vector2.Distance(item.transform.position, position);
                if (lastDistance > distance)
                {
                    selected = item;
                    lastDistance = distance;
                }
            }

            return selected;
        }

        public TItem GetItem(PointerEventData eventData)
        {
            if (!RectTransformUtility
                    .RectangleContainsScreenPoint(itemRoot, eventData.position, eventData.pressEventCamera))
                return null;

            RectTransformUtility
                .ScreenPointToWorldPointInRectangle(itemRoot, eventData.position, eventData.pressEventCamera, out var worldPos);

            TItem selected = null;
            float closest = float.MaxValue;
            foreach (TItem item in _items)
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

        protected abstract UniTask SetItem(TItem item, TData data, CancellationToken ct);

        private async UniTask<TItem> Add(TData data, int order, Action<UIItemSlot> onClick = null)
        {
            var item = await _itemPrefab.Instantiate<TItem>(itemRoot, Ct);
            if (item == null)
                return null;

            if (onClick != null)
            {
                item.OnClick -= onClick;
                item.OnClick += onClick;
            }

            _items.Add(item);
            await SetItem(item, data, Ct);
            return item;
        }
    }
}