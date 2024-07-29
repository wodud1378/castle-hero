using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

namespace RGLabs.Common.Behaviours
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject _dim;

        private readonly ReactiveCollection<PopupBase> _popups = new();

        private void Awake()
        {
            _dim.gameObject.SetActive(false);
            _popups
                .ChangeAsObservable()
                .Subscribe(OnPopupCollectionChanged)
                .AddTo(this);
        }
        
        public async UniTask<T> OpenAsync<T>(params object[] parameters) where T : PopupBase
        {
            var popup = await LoadPopup<T>();
            if (popup == null)
                return null;

            try { await popup.Open(parameters); }
            catch (Exception e)
            {
                _popups.Remove(popup);
                Addressables.ReleaseInstance(popup.gameObject);
                
                Debug.LogError(e);
                return null;
            }
            
            popup.gameObject.SetActive(true);
            return popup;
        }
        
        public async UniTask<T> OpenAsync<T>() where T : PopupBase
        {
            var popup = await LoadPopup<T>();
            if (popup == null)
                return null;
            
            await popup.Open();

            popup.gameObject.SetActive(true);
            return popup;
        }

        private async UniTask<T> LoadPopup<T>() where T : PopupBase
        {
            var path = PrefabPathCache.Load(typeof(T));
            if (string.IsNullOrEmpty(path))
                return null;
            
            var obj = await Addressables.InstantiateAsync(path, transform);
            if (!obj.TryGetComponent(out T popup))
            {
                Addressables.ReleaseInstance(obj);
                return null;    
            }
            
            _popups.Add(popup);
            popup.fromManager = true;
            popup.OnCloseEvent += OnClosed;
            return popup;
        }

        public async UniTask Close<T>(T popup) where T : PopupBase
            => await popup.CloseAsync();

        public async UniTask CloseAll()
        {
            var list = new List<UniTask>();
            foreach (var popup in _popups)
            {
                list.Add(popup.CloseAsync());
            }

            await UniTask.WhenAll(list);
        }

        private void OnClosed(PopupBase popup)
        {
            _popups.Remove(popup);
            
            popup.OnCloseEvent -= OnClosed;
            Addressables.ReleaseInstance(popup.gameObject);
        }

        private void OnPopupCollectionChanged(ReactiveCollection<PopupBase> collection)
        {
            _dim.gameObject.SetActive(collection.Count > 0);
            _dim.transform.SetSiblingIndex(_popups.Count - 1);
        }
    }
}