using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace CastleHero.View.Common
{
    public class PopupManager : MonoBehaviour, IPopupManager
    {
        [FormerlySerializedAs("_dim")]
        [SerializeField] private GameObject dim;

        private readonly ReactiveCollection<PopupBase> _popups = new();
        private readonly Dictionary<Type, GameObject> _prefabCache = new();

        private void Awake()
        {
            dim.gameObject.SetActive(false);
            _popups
                .ChangeAsObservable()
                .Subscribe(OnPopupCollectionChanged)
                .AddTo(this);
        }

        private void OnDestroy()
        {
            foreach (var prefab in _prefabCache.Values)
                Addressables.Release(prefab);
            _prefabCache.Clear();
        }

        public void Open<T>() where T : PopupBase
            => OpenAsync<T>().SafeForget();

        public void Open<T>(params object[] parameters) where T : PopupBase
            => OpenAsync<T>(parameters).SafeForget();

        public async UniTask<T> OpenAsync<T>(params object[] parameters) where T : PopupBase
        {
            var popup = LoadPopup<T>();
            if (popup == null)
                return null;

            try { await popup.Open(parameters); }
            catch (Exception e)
            {
                _popups.Remove(popup);
                UnityEngine.Object.Destroy(popup.gameObject);

                Debug.LogError(e);
                return null;
            }

            popup.gameObject.SetActive(true);
            return popup;
        }

        public async UniTask<T> OpenAsync<T>() where T : PopupBase
        {
            var popup = LoadPopup<T>();
            if (popup == null)
                return null;

            await popup.Open();

            popup.gameObject.SetActive(true);
            return popup;
        }

        public bool TryGetPopupIfExist<T>(out T popup) where T : PopupBase
        {
            popup = _popups.OfType<T>().FirstOrDefault();
            return popup != null;
        }

        public void ReplaceToTop(PopupBase popup)
        {
            popup.transform.SetAsLastSibling();
            dim.transform.SetSiblingIndex(_popups.Count - 1);
        }

        private T LoadPopup<T>() where T : PopupBase
        {
            var path = PrefabPathCache.Load(typeof(T));
            if (string.IsNullOrEmpty(path))
                return null;

            var type = typeof(T);
            if (!_prefabCache.TryGetValue(type, out var prefab))
            {
                // 최초 로드 시만 동기 블로킹 (1프레임). 이후 캐시에서 즉시 반환.
                var handle = Addressables.LoadAssetAsync<GameObject>(path);
                prefab = handle.WaitForCompletion();
                _prefabCache[type] = prefab;
            }

            var obj = UnityEngine.Object.Instantiate(prefab, transform);
            if (!obj.TryGetComponent(out T popup))
            {
                UnityEngine.Object.Destroy(obj);
                return null;
            }

            _popups.Add(popup);
            popup.fromManager = true;
            popup.OnCloseEvent += OnClosed;
            return popup;
        }

        public async UniTask Close<T>(T popup) where T : PopupBase
            => await popup.CloseAsync();

        public async UniTask CloseAllAsync()
        {
            var list = new List<UniTask>();
            var queue = new Queue<PopupBase>();
            foreach (var popup in _popups)
            {
                queue.Enqueue(popup);
            }

            while (queue.Count > 0)
            {
                list.Add(queue.Dequeue().CloseAsync());
            }

            await UniTask.WhenAll(list);

            _popups.Clear();
        }

        public void CloseAll() => CloseAllAsync().SafeForget();

        private void OnClosed(PopupBase popup)
        {
            _popups.Remove(popup);
            popup.OnCloseEvent -= OnClosed;
            UnityEngine.Object.Destroy(popup.gameObject);
        }

        private void OnPopupCollectionChanged(ReactiveCollection<PopupBase> collection)
        {
            dim.gameObject.SetActive(collection.Count > 0);
            dim.transform.SetSiblingIndex(_popups.Count - 1);
        }
    }
}
