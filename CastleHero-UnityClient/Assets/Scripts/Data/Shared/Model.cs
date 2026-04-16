using System;
using System.Collections.Generic;
using CastleHero;
using CastleHero.Data;
using CastleHero.Data.Model;
using Newtonsoft.Json;

using CastleHero.Common;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Network.Shared
{
    public class StaminaDto
    {
        public int point;
        public int pointLimit;
        public DateTime lastUpdate;
    }

    /// <summary>
    /// 재화 DTO. 순수 필드만 유지. 비즈니스 로직(ToItems, TryConsume* 등)은 CurrencyExtensions 로 분리.
    /// </summary>
    public class CurrencyDto
    {
        public int gold;
        public int freeDia;
        public int paidDia;

        public static CurrencyDto operator +(CurrencyDto a, CurrencyDto b)
            => new() { gold = a.gold + b.gold, freeDia = a.freeDia + b.freeDia, paidDia = a.paidDia + b.paidDia };

        public static CurrencyDto operator -(CurrencyDto a, CurrencyDto b)
            => new() { gold = a.gold - b.gold, freeDia = a.freeDia - b.freeDia, paidDia = a.paidDia - b.paidDia };
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
        public StaminaDto stamina;
        public CurrencyDto currency;
        public CharactersDto characters;
        public FormationDto formation;
        public InventoryDto inventory;
        public GameRecordDto gameRecord;
        public ShopRecordDto shopRecord;
    }

    #region Dungeon

    public class DungeonRecord
    {
        public int layer;
        public int lastClearedLv;
    }

    #endregion

    #region Shop

    public class Product
    {
        public struct BuyCount
        {
            public int byFree;
            public int byAd;
            public int byDefault;

            [JsonIgnore] public int Sum => byFree + byAd + byDefault;
        }

        public int shopId;
        public BuyCount total;
        public BuyCount current;
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
        public Pack pack;
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
        public int addedExp;
        public UnitInfo unit;

        public static UnitTransition Create(UnitInfo unit)
        {
            int rate = unit.rate;
            int exp = unit.exp;
            int lv = unit.lv;

            return new UnitTransition
            {
                rateTransition = new[] { rate, rate },
                expTransition = new[] { exp, exp },
                lvTransition = new[] { lv, lv },
                addedExp = 0,
                unit = unit,
            };
        }

        public static UnitTransition Create(UnitInfo unit, int lv, int exp, int addedExp)
        {
            return new UnitTransition
            {
                rateTransition = new[] { unit.rate, unit.rate },
                expTransition = new[] { unit.exp, exp },
                lvTransition = new[] { unit.lv, lv },
                addedExp = addedExp,
                unit = unit,
            };
        }

        public static UnitTransition Create(UnitInfo unit, int rate)
        {
            return new UnitTransition
            {
                rateTransition = new[] { unit.rate, rate },
                expTransition = new[] { unit.exp, unit.exp },
                lvTransition = new[] { unit.lv, unit.lv },
                addedExp = 0,
                unit = unit,
            };
        }
    }

    public class UnitGrowth
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

    public class Summon
    {
        public List<ISummoned> list;
    }

    #endregion

    #region Item.

    public class OpenBox
    {
        public CurrencyDto currency;
        public List<IItem> items;
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
        public int exp;
        public CurrencyDto currency;
        public List<IItem> items;
        public List<UnitTransition> transitions;

        public List<Reward> GetRewardsForDisplay()
        {
            var rewards = new List<Reward>();
            if (currency != null)
            {
                if (currency.gold > 0)
                    rewards.Add(new Reward { icon = ServiceLocator.Get<GameConstants>().goldIcon, min = currency.gold, max = currency.gold, percent = 1f });
                int dia = currency.freeDia + currency.paidDia;
                if (dia > 0)
                    rewards.Add(new Reward { icon = ServiceLocator.Get<GameConstants>().diaIcon, min = dia, max = dia, percent = 1f });
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    string icon = ServiceLocator.Get<IDBProvider>().Items.TryFind(item.ItemId, out var entity) ? entity.icon : string.Empty;
                    rewards.Add(new Reward { icon = icon, id = item.ItemId, min = item.Quantity, max = item.Quantity, percent = 1f });
                }
            }

            return rewards;
        }
    }

    #endregion
}