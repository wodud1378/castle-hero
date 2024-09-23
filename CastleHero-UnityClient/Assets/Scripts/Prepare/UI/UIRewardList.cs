using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Prepare.UI
{
    public struct Reward
    {
        public string icon;
        public int id;
        public int min;
        public int max;
        public float percent;
    }

    public class UIRewardList : UIListAdapter<UISlot, Reward>
    {
        public UniTask Init(IGameEntity entity)
        {
            var rewards = new List<Reward>();

            if (entity is { MinGold: > 0, MaxGold: > 0 })
            {
                rewards.Add(new()
                {
                    icon = Constants.GoldIcon,
                    min = entity.MinGold,
                    max = entity.MaxGold,
                    percent = 1f
                });
            }

            if (entity.Exp > 0)
            {
                rewards.Add(new()
                {
                    icon = Constants.ExpIcon,
                    min = entity.Exp,
                    max = entity.Exp,
                    percent = 1f
                });
            }

            rewards.AddRange(entity.GetRewardsForDisplay());

            return base.Init(rewards);
        }

        protected override UniTask SetItem(UISlot slot, Reward data)
        {
            var text = data.min != data.max
                ? $"{data.min:N0}~{data.max:N0}"
                : data.min > 1
                    ? $"{data.min:N0}"
                    : string.Empty;

            var task = slot.Init(data.icon, text);

            slot.OnClick += (s) => OnClickSlot(s, data);

            return task;
        }

        private void OnClickSlot(UISlot slot, Reward data)
        {
            string QuantityText(Reward reward) => reward.min == reward.max
                ? $"{reward.min:N0}"
                : $"{reward.min:N0}~{reward.max:N0}";
            
            var sb = new StringBuilder();
            if (data.id != 0 && Storage.db.items.TryFind(data.id, out var entity))
            {
                switch (entity.type)
                {
                    case ItemType.Chest:
                        var option = entity.optionChestFull;
                        bool isFirst = true;
                        foreach (var kvp in option.itemMap)
                        {
                            if (!Storage.db.items.TryFind(kvp.Key, out var e))
                                continue;

                            if (isFirst)
                                isFirst = false;
                            else
                                sb.AppendLine();

                            sb.Append($"{e.name}");
                        }

                        break;
                    default:
                        sb.Append($"{entity.name} {QuantityText(data)}");
                        break;
                }
            }
            else if(data.icon == Constants.ExpIcon)
            {
                sb.Append($"{Storage.localize.Get(250)} {QuantityText(data)}");
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