using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Utility;

namespace RGLabs.Lobby.UI.Inventory
{
    public class UIInventory : UIListAdapter<UIItemSlot, IItem>
    {
        public enum Tab
        {
            All,
            Equipment,
            Other
        }

        public async UniTask Init(Tab tab)
        {
            var compareMethod = CompareMethod(tab);
            if (compareMethod == null)
                return;
            
            var items = Storage.userRepository.items;
            var filtered = items
                .Where(x => compareMethod.Invoke(x));

            await base.Init(filtered);
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

        protected override async UniTask SetItem(UIItemSlot item, IItem data)
        {
            var accessor = Storage.db.itemDBAccessor;
            if (!accessor.TryLoad(data.Id, out var entity))
                return;
            
            await item.InitAsync(entity.Icon);
        }
    }
}