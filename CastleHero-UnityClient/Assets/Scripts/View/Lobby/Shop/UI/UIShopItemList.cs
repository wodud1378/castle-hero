using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CastleHero.View.Lobby.Shop.UI
{
    public class UIShopItemList : UIListAdapter<UIShopItemSlot, ShopItemEntity>
    {
        private readonly Dictionary<string, GameObject> _prefabCache = new();

        protected override UIShopItemSlot ProvideSlot(ShopItemEntity data)
        {
            if (!_prefabCache.TryGetValue(data.prefab, out var prefab))
            {
                prefab = Addressables.LoadAssetAsync<GameObject>(data.prefab).WaitForCompletion();
                _prefabCache[data.prefab] = prefab;
            }

            var obj = UnityEngine.Object.Instantiate(prefab, itemRoot);
            return obj.TryGetComponent(out UIShopItemSlot slot) ? slot : null;
        }

        private void OnDestroy()
        {
            foreach (var prefab in _prefabCache.Values)
                Addressables.Release(prefab);
            _prefabCache.Clear();
        }

        protected override UniTask SetItem(UIShopItemSlot slot, ShopItemEntity data) => slot.Init(data);
    }
}
