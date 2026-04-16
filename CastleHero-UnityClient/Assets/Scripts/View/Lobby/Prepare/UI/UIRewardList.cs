using System.Collections.Generic;
using System.Text;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using UnityEngine;
using CastleHero.Common.Pattern;

using CastleHero.Data.DB;
using CastleHero.Common.Localize;
namespace CastleHero.View.Lobby.Prepare.UI
{
    using Reward = CastleHero.Data.Model.Reward;

    public class UIRewardList : UIListAdapter<UISlot, Reward>
    {
        public new UniTask Init(IEnumerable<Reward> rewards) => base.Init(rewards);

        public UniTask Init(IGameEntity entity)
        {
            var rewards = new List<Reward>();

            if (entity is { MinGold: > 0, MaxGold: > 0 })
            {
                rewards.Add(new()
                {
                    icon = ServiceLocator.Get<GameConstants>().goldIcon,
                    min = entity.MinGold,
                    max = entity.MaxGold,
                    percent = 1f
                });
            }

            if (entity.Exp > 0)
            {
                rewards.Add(new()
                {
                    icon = ServiceLocator.Get<GameConstants>().expIcon,
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
            string AmountText(Reward reward)
            {
                string quantityText = reward.min == reward.max
                    ? $"{reward.min:N0}"
                    : $"{reward.min:N0}~{reward.max:N0}";
                
                // TODO : "획득" 텍스트 추가 후 -1 아이디를 해당 id로 교체.
                return $"{quantityText} {ServiceLocator.Get<LocalizeText>().Get(-1)}";
            }

            var sb = new StringBuilder();
            if (data.id != 0 && ServiceLocator.Get<IDBProvider>().Items.TryFind(data.id, out var entity))
            {
                switch (entity.type)
                {
                    case ItemType.Chest:
                        var option = entity.optionChestFull;
                        bool isFirst = true;
                        foreach (var kvp in option.itemMap)
                        {
                            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(kvp.Key, out var e))
                                continue;

                            if (isFirst)
                                isFirst = false;
                            else
                                sb.AppendLine();

                            sb.Append($"{e.name}");
                        }

                        break;
                    default:
                        sb.Append($"{entity.name} {AmountText(data)}");
                        break;
                }
            }
            else if(data.icon == ServiceLocator.Get<GameConstants>().expIcon)
            {
                sb.Append($"{ServiceLocator.Get<LocalizeText>().Get(250)} {AmountText(data)}");
            }

            var infoString = sb.ToString();

            ServiceLocator.Get<UIToolTip>().Open(
                infoString,
                slot.transform as RectTransform,
                0f,
                1f);
        }
    }
}