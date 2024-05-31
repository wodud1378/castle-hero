using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Common.Behaviours
{
    public class PopupManager : MonoBehaviour
    {
        [SerializeField] private GameObject _dim;

        private readonly ReactiveCollection<PopupBase> _popups = new();

        private void Awake()
        {
            _popups
                .ChangeAsObservable()
                .Subscribe(OnPopupCollectionChanged)
                .AddTo(this);
        }

        public async UniTask<T> Open<T>() where T : PopupBase
        {
            var path = PrefabPathCache.Load(typeof(T));
            if (string.IsNullOrEmpty(path))
                return null;

            var obj = await Addressables.InstantiateAsync(path);
            if (!obj.TryGetComponent(out T popup))
            {
                Addressables.ReleaseInstance(obj);
                return null;    
            }

            _popups.Add(popup);

            popup.OnClose += OnClosePopup;
            await popup.Open();
            return popup;
        }

        public async UniTask Close<T>(T popup) where T : PopupBase
            => await popup.Close();

        public async UniTask CloseAll()
        {
            var list = new List<UniTask>();
            foreach (var popup in _popups)
            {
                list.Add(popup.Close());
            }

            await UniTask.WhenAll();
        }

        private void OnClosePopup(PopupBase popup)
        {
            _popups.Remove(popup);
            popup.OnClose -= OnClosePopup;
        }

        private void OnPopupCollectionChanged(ReactiveCollection<PopupBase> collection)
        {
            _dim.transform.SetSiblingIndex(_popups.Count);
        }
    }
}