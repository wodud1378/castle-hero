using System;
using RGLabs.Data.Model;
using RGLabs.Network.Model;
using RGLabs.Unit;
using RGLabs.Utility;
using TMPro;
using UnityEngine;

namespace RGLabs.Lobby.UI
{
    public class UIEquipmentItem : UIItemInformation<EquipItem, EquipmentEntity>
    {
        [Serializable]
        public struct UIStat
        {
            public Status.Type type;
            public TMP_Text label;
        }

        [SerializeField] private UIStat[] _uiStats;
        
        protected override void Construct(EquipItem item, EquipmentEntity entity)
        {
            int index = 0;
            while (index.IsValidIndex(item.stats, item.values))
            {
                var type = (Status.Type)item.stats[index];
                var value = item.values[index];

                var ui = Array.Find(_uiStats, x => x.type == type);
                ui.label.text = $"+{(int)(value * 100)} %";
                ++index;
            }
        }
    }
}