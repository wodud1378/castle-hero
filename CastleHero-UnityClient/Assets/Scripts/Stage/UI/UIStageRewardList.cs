using System;
using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Stage.UI
{
    public struct StageReward
    {
        public string icon;
        public int id;
        public int min;
        public int max;
        public float percent;
    }
    
    public class UIStageRewardList : UIListAdapter<UISlot, StageReward>
    {
        public UniTask Init(StageEntity entity)
        {
            var rewards = new List<StageReward>();
            
            if (entity is { MinGold: > 0, MaxGold: > 0 })
                rewards.Add(new ()
                {
                    icon = Constants.GoldIcon,
                    min = entity.MinGold,
                    max = entity.MaxGold,
                    percent = 1f
                });

            if (entity.exp > 0)
                rewards.Add(new ()
                {
                    icon = Constants.ExpIcon,
                    min = entity.exp,
                    max = entity.exp,
                    percent = 1f
                });

            if (Storage.db.items.TryFind(entity.propItemId, out var itemEntity))
                rewards.Add(new ()
                {
                    icon = itemEntity.icon,
                    id = itemEntity.Id,
                    min = entity.propItemQty,
                    max = entity.propItemQty,
                    percent = entity.itemPer
                });

            return base.Init(rewards);
        }
        
        protected override UniTask SetItem(UISlot slot, StageReward data)
        {
            var task = slot.Init(data.icon);
            slot.OnClick += (s)=> OnClickSlot(s, data);

            return task;
        }

        private void OnClickSlot(UISlot slot, StageReward data)
        {
            var sb = new StringBuilder();
            if (data.id != 0 && Storage.db.items.TryFind(data.id, out var entity))
            {
                switch (entity.type)
                {
                    case ItemType.Chest:
                        var option = entity.GetChestOption();
                        bool isFirst = true;
                        foreach (var id in option.items)
                        {
                            if (!Storage.db.items.TryFind(id, out var e))
                                continue;
                            
                            if (isFirst)
                                isFirst = false;
                            else
                                sb.AppendLine();

                            sb.Append($"{e.name}");
                        }
                        break;
                    default:
                        sb.Append(entity.name);
                        break;
                }
            }
            else
            {
                string text = data.min == data.max
                    ? $"{data.min:N0}"
                    : $"{data.min:N0}~{data.max:N0}";

                sb.Append(text);
            }

            var infoString = sb.ToString();
            
            Context.toolTip.Open(
                infoString, 
                slot.transform as RectTransform,
                0f,
                1f);
        }
    }
}