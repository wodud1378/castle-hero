using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.UI
{
    public abstract class UIListAdapter<TItem, TData> : MonoBehaviour, IDisposable
        where TItem : UIItemSlot
    {
        [SerializeField] protected RectTransform itemRoot;
        
        [SerializeField] private AssetReference _itemPrefab;
        
        public bool IsOpen { get; protected set; }
        
        private readonly List<TItem> _items = new();

        public async UniTask Init(IEnumerable<TData> array)
        {
            Clear();
            
            var tasks = new List<UniTask>();
            int order = 0;
            foreach (var data in array)
            {
                tasks.Add(Add(data, order++));
            }

            await UniTask.WhenAll(tasks);
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

        public void Dispose() => Clear();

        protected void Clear()
        {
            _items.ForEach(x=>
            {
                x.Dispose();
                Addressables.ReleaseInstance(x.gameObject);
            });
            
            _items.Clear();
        }
        
        protected abstract UniTask SetItem(TItem item, TData data);

        private async UniTask<TItem> Add(TData data, int order)
        {
            var obj = await Addressables.InstantiateAsync(_itemPrefab, itemRoot);
            var item = obj.GetComponent<TItem>();
            if (item == null)
                return null;

            await SetItem(item, data);
            return item;
        }
    }
}