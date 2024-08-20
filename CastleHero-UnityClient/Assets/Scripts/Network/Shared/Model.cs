using System;
using System.Collections.Generic;
using RGLabs.Common;
using RGLabs.Data;
using RGLabs.Prepare.UI;
using RGLabs.Utility;

namespace RGLabs.Network.Shared
{
    public class ProfileDto
    {
        public int iconId;
        public int stage;
        public int focusedStage;
        public int castleLv;
    }

    public class ActDto
    {
        public int point;
        public int pointLimit;
        public DateTime lastUpdate;
    }

    public class CurrencyDto
    {
        public int gold;
        public int freeDia;
        public int paidDia;

        public static CurrencyDto operator +(CurrencyDto a, CurrencyDto b)
        {
            return new CurrencyDto
            {
                gold = a.gold + b.gold,
                freeDia = a.freeDia + b.freeDia,
                paidDia = a.paidDia + b.paidDia
            };
        }

        public static CurrencyDto operator -(CurrencyDto a, CurrencyDto b)
        {
            return new CurrencyDto
            {
                gold = a.gold - b.gold,
                freeDia = a.freeDia - b.freeDia,
                paidDia = a.paidDia - b.paidDia
            };
        }

        public List<IItem> ToItems()
        {
            return new List<IItem>
            {
                new Item { ItemId = Constants.PaidDiaId, Quantity = paidDia },
                new Item { ItemId = Constants.FreeDiaId, Quantity = freeDia },
                new Item { ItemId = Constants.GoldId, Quantity = gold },
            };
        }

        public bool IsEmpty() => gold > 0 || freeDia > 0 || paidDia > 0;
    }

    public class CharactersDto
    {
        public List<UnitInfo> units;
    }

    public class FormationDto
    {
        public List<FieldUnit> fieldUnits;
    }

    public class InventoryDto
    {
        public List<IItem> items;
    }

    public class GameRecordDto
    {
        public int iconId;
        public int castleLv;
        public int lastClearedStage;
        public List<DungeonRecord> dungeon;
    }

    public class ShopRecordDto
    {
        public class History
        {
            public int id;
            public int type;
            public DateTime time;
        }

        public List<Product> products;
        public List<History> histories;
    }


    public class UserDataDto
    {
        public ActDto act;
        public CurrencyDto currency;
        public CharactersDto characters;
        public FormationDto formation;
        public InventoryDto inventory;
        public GameRecordDto gameRecord;
        public ShopRecordDto shopRecord;
    }

    #region Shop

    public class Purchase
    {
        public struct History
        {
            public int type;
            public DateTime at;
        }

        public int shopId;
        public int byDefault;
        public int byAd;
        public int byFree;
        public DateTime nextReset;
        public List<History> histories;
    }

    #endregion

    #region Dungeon

    public class DungeonRecord
    {
        public int type;
        public int lv;
        public DateTime lastTime;
    }

    #endregion

    #region Shop

    public class Product
    {
        public int shopId;
        public int byDefault;
        public int byAd;
        public int byFree;
        public DateTime nextReset;
        public DateTime expireDate;
        public DateTime updatedAt;
    }

    public class Pack
    {
        public CurrencyDto currency;
        public List<IItem> items;
        public List<int> unitIds;
    }

    public class ItemBought
    {
        public int shopId;
        public Pack pack;
        public ShopRecordDto record;
    }

    public class ItemsSold
    {
        public CurrencyDto currency;
        public List<IItem> items;
    }

    #endregion

    #region Unit.

    public class FieldUnit
    {
        public int id;
        public float x;
        public float y;
    }

    public class UnitInfo
    {
        public int id;
        public int lv;
        public int rate;
        public int exp;

        public List<string> equipments;
    }

    public class UnitTransition
    {
        public int[] rateTransition;
        public int[] lvTransition;
        public int[] expTransition;
        public UnitInfo unit;

        public static UnitTransition Create(UnitInfo unit)
        {
            int rate = unit.rate;
            int exp = unit.exp;
            int lv = unit.lv;

            return new UnitTransition
            {
                rateTransition = new int[] { rate, rate },
                expTransition = new int[] { exp, exp },
                lvTransition = new int[] { lv, lv },
                unit = unit
            };
        }

        public static UnitTransition Create(UnitInfo unit, int lv, int exp)
        {
            return new UnitTransition
            {
                rateTransition = new int[] { unit.rate, unit.rate },
                expTransition = new int[] { unit.exp, exp },
                lvTransition = new int[] { unit.lv, lv },
                unit = unit
            };
        }

        public static UnitTransition Create(UnitInfo unit, int rate)
        {
            return new UnitTransition
            {
                rateTransition = new int[] { unit.rate, rate },
                expTransition = new int[] { unit.exp, unit.exp },
                lvTransition = new int[] { unit.lv, unit.lv },
                unit = unit
            };
        }
    }

    public class GrowthResult
    {
        public UnitTransition transition;
        public CurrencyDto leftCurrency;
        public IItem leftItem;
    }

    #endregion

    #region Castle

    public class CastleGrowth
    {
        public int lv;
        public CurrencyDto leftCurrency;
    }

    #endregion

    #region Summon.

    public interface ISummoned
    {
        public int Id { get; }
    }

    public class SummonedUnit : ISummoned
    {
        public int Id { get; set; }
        public int lv;
        public int rate;
    }

    public class SummonedSoul : ISummoned
    {
        public int Id { get; set; }
        public int quantity;
    }

    public class SummonResult
    {
        public CurrencyDto leftCurrency;
        public IItem leftItem;
        public List<ISummoned> summoneds;
    }

    #endregion


    #region Item.

    public class OpenBoxResult
    {
        public CurrencyDto currency;
        public List<IItem> items;
        public IItem leftItem;
    }

    public class Consume
    {
        public enum Method
        {
            Sell,
            Use,
            Equip,
        }

        public Method method;
        public int id;
        public int quantity;
    }

    public class ConsumeResult
    {
        public int id;
        public int consumed;
        public int left;
    }

    public interface IItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    public class EquipItem : IItem
    {
        public class Stat
        {
            public int type;
            public float value;
        }

        public class Element
        {
            public int type;
            public int lv;
        }

        public string Guid { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public int character;
        public int slot;

        public Stat main;
        public List<Stat> sub;
        public Element element;
    }

    public class Item : IItem
    {
        public int ItemId { get; set; }
        public int Quantity { get; set; }
    }

    #endregion

    #region InGame.

    public class GameCleared
    {
        public int id;
        public bool isFirstClear;
        public CurrencyDto currency;
        public List<IItem> items;

        public virtual List<Reward> GetRewardItems()
        {
            var rewards = new List<Reward>();
            if (!currency.IsEmpty())
            {
                var currencyItems = currency.ToItems();
                currencyItems.ForEach(x =>
                {
                    if (x.Quantity == 0)
                        return;

                    rewards.Add(new()
                    {
                        icon = x.ItemId == Constants.GoldId ? Constants.GoldIcon : Constants.DiaIcon,
                        id = x.ItemId,
                        min = x.Quantity,
                        max = x.Quantity,
                        percent = 1f
                    });
                });
            }

            items.ForEach(x =>
            {
                if (!Storage.db.items.TryFind(x.ItemId, out var entity))
                    return;

                rewards.Add(new()
                {
                    icon = entity.icon,
                    id = x.ItemId,
                    min = x.Quantity,
                    max = x.Quantity,
                    percent = 1f
                });
            });

            return rewards;
        }
    }

    public class DungeonCleared : GameCleared
    {
    }

    public class StageCleared : GameCleared
    {
        public int exp;
        public List<UnitTransition> transitions;

        public override List<Reward> GetRewardItems()
        {
            var list = base.GetRewardItems();
            if (exp > 0)
            {
                int index = list.FindLastIndex(
                    x => x.id == Constants.GoldId ||
                         x.id == Constants.FreeDiaId ||
                         x.id == Constants.PaidDiaId);

                if (!index.IsValidIndex(list))
                    index = 0;

                list.Insert(index, new()
                {
                    icon = Constants.ExpIcon,
                    min = exp,
                    max = exp,
                    percent = 1f
                });
            }

            return list;
        }
    }

    #endregion
}