using System.Collections.Generic;
using System.Text;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.Common.Localize;
using CastleHero.Common.Pattern;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.View.Common.UI;
using UnityEngine;

namespace CastleHero.View.Lobby.Prepare.UI
{
    using Reward = CastleHero.Data.Model.Reward;

    public class UIRewardList : UIListAdapter<UISlot, Reward>
    {
        private IDBProvider _db;
        private GameConstants _constants;
        private LocalizeText _localize;
        private UIToolTip _toolTip;

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            _db = sl.Get<IDBProvider>();
            _constants = sl.Get<GameConstants>();
            _localize = sl.Get<LocalizeText>();
            _toolTip = sl.Get<UIToolTip>();
        }

        public new void Init(IEnumerable<Reward> rewards) => base.Init(rewards);

        public void Init(IGameEntity entity)
        {
            var rewards = new List<Reward>();

            if (entity is { MinGold: > 0, MaxGold: > 0 })
            {
                rewards.Add(new()
                {
                    icon = _constants.goldIcon,
                    min = entity.MinGold,
                    max = entity.MaxGold,
                    percent = 1f
                });
            }

            if (entity.Exp > 0)
            {
                rewards.Add(new()
                {
                    icon = _constants.expIcon,
                    min = entity.Exp,
                    max = entity.Exp,
                    percent = 1f
                });
            }

            rewards.AddRange(entity.GetRewardsForDisplay(_db));

            base.Init(rewards);
        }

        protected override void SetItem(UISlot slot, Reward data)
        {
            var text = data.min != data.max
                ? $"{data.min:N0}~{data.max:N0}"
                : data.min > 1
                    ? $"{data.min:N0}"
                    : string.Empty;

            slot.Init(data.icon, text);

            slot.OnClick += (s) => OnClickSlot(s, data);
        }

        private void OnClickSlot(UISlot slot, Reward data)
        {
            string AmountText(Reward reward)
            {
                string quantityText = reward.min == reward.max
                    ? $"{reward.min:N0}"
                    : $"{reward.min:N0}~{reward.max:N0}";

                return $"{quantityText} {_localize.Get(-1)}";
            }

            var sb = new StringBuilder();
            if (data.id != 0 && _db.Items.TryFind(data.id, out var entity))
            {
                switch (entity.type)
                {
                    case ItemType.Chest:
                        var option = entity.optionChestFull;
                        bool isFirst = true;
                        foreach (var kvp in option.itemMap)
                        {
                            if (!_db.Items.TryFind(kvp.Key, out var e))
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
            else if(data.icon == _constants.expIcon)
            {
                sb.Append($"{_localize.Get(250)} {AmountText(data)}");
            }

            var infoString = sb.ToString();

            _toolTip.Open(
                infoString,
                slot.transform as RectTransform,
                0f,
                1f);
        }
    }
}
