using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data.Model;
using RGLabs.Lobby.UI.Popup;
using RGLabs.Network.Model;
using RGLabs.Unit;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Inventory.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popup_Equipment.prefab")]

    public class PopupEquipItem : PopupItemBase<UIEquipmentSlot, EquipItem, EquipmentEntity>
    {
        [SerializeField] private UIStatusText[] _stats;

        [SerializeField] private Button _refine;
        [SerializeField] private Button _equip;
        [SerializeField] private Button _release;

        protected override void InitSubscriptions()
        {
            base.InitSubscriptions();
            
            this.SubscribeButton(_refine, OpenElementalStoneList);
            this.SubscribeButton(_equip, OpenCharacterList);
            this.SubscribeButton(_release, Release);
        }

        protected override UniTask InitSlot(UIEquipmentSlot slot) => slot.Init(Item, Entity);

        private void OpenElementalStoneList()
        {
        }
        
        private void OpenCharacterList()
        {
            Context.popupManager
                .Open<PopupCharacterList>()
                .Forget();
        }

        private void Release()
        {
        }

        protected override void OnDataInitialized()
        {
            int index = 0;
            while (index.IsValidIndex(Item.stats, Item.values))
            {
                var label = Array.Find(_stats,
                    x => x.type == (Status.Type)Item.stats[index]);

                if (label != null)
                {
                    label.SetText(Item.values[index]);
                }

                ++index;
            }
        }
    }
}