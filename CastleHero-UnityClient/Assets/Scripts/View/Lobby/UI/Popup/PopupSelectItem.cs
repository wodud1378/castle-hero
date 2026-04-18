using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.Lobby.UI.Adapter;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Popup_SelectItem.prefab")]
    public class PopupSelectItem : PopupBase, ISelect<IItem>
    {
        [FormerlySerializedAs("_itemList")]
        [SerializeField] private UIInventoryItemList itemList;

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

            itemList.OnSlotClickEvent += OnClickItemSlot;

            itemList.Init(items);
            return UniTask.CompletedTask;
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
