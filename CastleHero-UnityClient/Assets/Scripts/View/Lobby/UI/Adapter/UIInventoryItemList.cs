using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.View.Lobby.UI.Inventory;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.View.Lobby.UI.Adapter
{
    public class UIInventoryItemList : UIListAdapter<UIItemSlot, IItem>
    {
        [FormerlySerializedAs("_equipItemPrefab")]
        [SerializeField] private AssetReference equipItemPrefab;

        private GameObject _cachedEquipPrefab;

        protected override UIItemSlot ProvideSlot(IItem data)
        {
            if (data.ItemId.IsEquipItem())
            {
                if (_cachedEquipPrefab == null)
                    _cachedEquipPrefab = equipItemPrefab.LoadAssetAsync<GameObject>().WaitForCompletion();

                var obj = UnityEngine.Object.Instantiate(_cachedEquipPrefab, itemRoot);
                return obj.TryGetComponent(out UIEquipmentSlot slot) ? slot : null;
            }

            return base.ProvideSlot(data);
        }

        private void OnDestroy()
        {
            if (_cachedEquipPrefab != null)
            {
                Addressables.Release(_cachedEquipPrefab);
                _cachedEquipPrefab = null;
            }
        }

        protected override UniTask SetItem(UIItemSlot slot, IItem data)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(data.ItemId, out var entity))
                return UniTask.CompletedTask;

            if (slot is UIEquipmentSlot eSlot && data is EquipItem eItem)
                return eSlot.Init(eItem);

            return slot.Init(data, entity);
        }
    }
}
