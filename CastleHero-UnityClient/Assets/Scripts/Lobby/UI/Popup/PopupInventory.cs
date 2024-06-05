using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Common.UI.Popup;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Inventory.prefab")]
    public class PopupInventory : PopupBase
    {
        public enum Tab
        {
            All,
            Equipment,
            Other
        }

        [SerializeField] private UIInventoryItemList _itemList;
        [SerializeField] private ToggleGroup _tabToggle;

        public readonly ReactiveProperty<Tab> tab = new();

        private CancellationTokenSource _ctSource;
        
        protected override void OnAwake()
        {
            base.OnAwake();

            this.UpdateAsObservable()
                .Select(_ => _tabToggle.ActiveToggles().FirstOrDefault(t => t.isOn))
                .DistinctUntilChanged()
                .Select(toggle => Enum.Parse<Tab>(toggle.gameObject.name))
                .Subscribe(selected => tab.Value = selected)
                .AddTo(this);
            
            Storage.userRepository.items
                .ChangeAsObservable()
                .Subscribe(_=> UpdateUI())
                .AddTo(this);

            tab
                .DistinctUntilChanged()
                .Subscribe(_=> UpdateUI())
                .AddTo(this);
        }

        public override UniTask Open(params object[] parameters)
        {
            Tab tabParam;
            try { tabParam = (Tab)parameters[0]; }
            catch { tabParam = default; }

            tab.Value = tabParam;
            
            _ctSource = new();

            return UpdateUI();
        }

        public override UniTask Open() => Open(Tab.All);

        protected override void OnClose()
        {
            if (_ctSource != null)
            {
                _ctSource.Cancel();
                _ctSource.Dispose();    
            }
        }

        private UniTask UpdateUI()
        {
            var items = Storage.userRepository.items
                .Where(CompareMethod(tab.Value).Invoke);

            return _itemList.Init(items);
        }
        
        private Predicate<IItem> CompareMethod(Tab tab)
        {
            return tab switch
            {
                Tab.All => _ => true,
                Tab.Equipment => x => x.Id.ItemType() == ItemTypeCode.Equipment,
                Tab.Other => x => x.Id.ItemType() != ItemTypeCode.Equipment,
                _ => null
            };
        }
    }
}