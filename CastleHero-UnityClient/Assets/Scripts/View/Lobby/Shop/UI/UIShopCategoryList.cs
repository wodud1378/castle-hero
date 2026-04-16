using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Shop.UI
{
    public class UIShopCategoryList : UIListAdapter<UIShopCategorySlot, IEnumerable<ShopItemEntity>>
    {
        [FormerlySerializedAs("_categorySlots")]
        [SerializeField] private AssetReference[] categorySlots;

        private GameObject[] _cachedCategoryPrefabs;

        protected override UIShopCategorySlot ProvideSlot(IEnumerable<ShopItemEntity> data)
        {
            int index = (int)data.First().category;

            _cachedCategoryPrefabs ??= new GameObject[categorySlots.Length];

            if (_cachedCategoryPrefabs[index] == null)
                _cachedCategoryPrefabs[index] = categorySlots[index].LoadAssetAsync<GameObject>().WaitForCompletion();

            var obj = UnityEngine.Object.Instantiate(_cachedCategoryPrefabs[index], itemRoot);
            return obj.TryGetComponent(out UIShopCategorySlot slot) ? slot : null;
        }

        private void OnDestroy()
        {
            if (_cachedCategoryPrefabs == null)
                return;

            foreach (var prefab in _cachedCategoryPrefabs)
            {
                if (prefab != null)
                    Addressables.Release(prefab);
            }
        }

        protected override UniTask SetItem(UIShopCategorySlot slot, IEnumerable<ShopItemEntity> data) => slot.Init(data);
    }
}
