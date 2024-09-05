using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_SelectItem.prefab")]
    public class PopupSelectItem : PopupBase, ISelect<IItem>
    {
        [SerializeField] private UIInventoryItemList _itemList;
        
        public UniTask<IItem> SelectTask => _ctSource.Task;

        private UniTaskCompletionSource<IItem> _ctSource;
        private bool _closeAfterSelect;

        public void BeginSelect(bool closeAfterSelect)
        {
            _ctSource = new UniTaskCompletionSource<IItem>();
            _closeAfterSelect = closeAfterSelect;
        }

        public override UniTask Open(params object[] parameters)
        {
            if (parameters[0] is not IEnumerable<IItem> items)
            {
                Close();
                return UniTask.CompletedTask;
            }

            _itemList.OnSlotClickEvent += OnClickItemSlot;
            
            return _itemList.Init(items);
        }

        protected override void OnClose()
        {
            base.OnClose();

            TrySelectComplete(null);
        }


        private void OnClickItemSlot(UIItemSlot slot)
        {
            if (!TrySelectComplete(slot.Item))
                return;

            if (_closeAfterSelect)
                Close();
        }

        private bool TrySelectComplete(IItem item)
        {
            if (_ctSource == null)
                return false;

            _ctSource.TrySetResult(item);
            _ctSource = null;
            return true;
        }
    }
}