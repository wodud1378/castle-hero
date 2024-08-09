using System;
using System.Collections.Generic;
using RGLabs.Common;

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

        public List<Item> ToItems()
        {
            return new List<Item>
            {
                new() { ItemId = Constants.PaidDiaId, Quantity = paidDia },
                new() { ItemId = Constants.FreeDiaId, Quantity = freeDia },
                new() { ItemId = Constants.GoldId, Quantity = gold },
            };
        }
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

    public class UserDataDto
    {
        public ProfileDto profile;
        public ActDto act;
        public CurrencyDto currency;
        public CharactersDto characters;
        public FormationDto formation;
        public InventoryDto inventoryDto;
    }

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

    #endregion

    #region Item.

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
}