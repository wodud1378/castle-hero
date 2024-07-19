using System;
using System.Collections.Generic;

namespace RGLabs.Network.Shared
{
    public abstract class Data
    {
        public string owner_inDate;
        public string inDate;
    }

    public class Info
    {
        public int stage;
        public int focusedStage;
        public int castleLv;
    }

    public class Act
    {
        public int point;
        public int pointMax;
        public DateTime lastUsage;
    }

    public class Currency
    {
        public int gold;
        public int freeDia;
        public int paidDia;
    }

    public class Characters
    {
        public List<UnitInfo> units;
    }

    public class Formation
    {
        public List<FieldUnit> fieldUnits;
    }

    public class Inventory
    {
        public List<IItem> items;
    }

    public class UserData
    {
        public Info info;
        public Act act;
        public Currency currency;
        public Characters characters;
        public Formation formation;
        public Inventory inventory;
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