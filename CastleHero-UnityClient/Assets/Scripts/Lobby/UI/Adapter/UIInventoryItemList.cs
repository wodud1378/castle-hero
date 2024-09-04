using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Lobby.UI.Inventory;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Lobby.UI.Adapter
{
    public class UIInventoryItemList : UIListAdapter<UIItemSlot, IItem>
    {
        [SerializeField] private AssetReference _equipItemPrefab;
        protected override async UniTask<UIItemSlot> ProvideSlot(IItem data)
        {
            var slot = data.ItemId.IsEquipItem()
                ? await _equipItemPrefab.Instantiate<UIEquipmentSlot>(itemRoot)
                : await base.ProvideSlot(data);

            return slot;
        }

        protected override UniTask SetItem(UIItemSlot slot, IItem data)
        {
            if (!Storage.db.items.TryFind(data.ItemId, out var entity))
                return UniTask.CompletedTask;

            if (slot is UIEquipmentSlot eSlot && data is EquipItem eItem)
                return eSlot.Init(eItem);
            
            return slot.Init(data, entity);
        }
    }
}