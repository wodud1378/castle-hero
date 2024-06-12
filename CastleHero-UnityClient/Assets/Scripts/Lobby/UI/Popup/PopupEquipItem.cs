using System;
using Cysharp.Threading.Tasks;
using RGLabs.Common.Behaviours;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Unit;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI.Popup
{
    public class PopupEquipItem : PopupItemBase<EquipItem, EquipmentEntity>
    {
        [Serializable]
        public class UIGrade
        {
            public EquipmentGradeCode grade;
            public GameObject obj;
        }
        
        [SerializeField] private UIGrade[] _grades;
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
            foreach (var e in _grades)
            {
                e.obj.SetActive(e.grade == (EquipmentGradeCode)Entity.grade);
            }

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