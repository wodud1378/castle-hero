using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.DB;
using RGLabs.Unit.Components;
using RGLabs.Utility;

namespace RGLabs.Data.Model
{
    public enum EquipmentGrade
    {
        Common = 0,
        Rare = 1,
        Epic = 2,
        Legend = 3,
    }

    public enum ConsumeType
    {
        Stamina = 1,
        Exp = 2,
        PlayTicket = 3,
        SummonTicket = 4,
        ElementalStone = 5,
    }

    public enum IngredientType
    {
        Soul = 1,
        ElementalPiece = 2,
        EquipmentPiece = 3,
    }

    public enum EquipmentSlot
    {
        Weapon = 0,
        Armor = 1,
        Ring = 2,
        Necklace = 3,
        Count,
    }

    public enum ItemType
    {
        Equipment = 0,
        Consumable,
        Ingredient,
        Chest,
    }

    public partial struct ItemEntity : IEntity
    {
        [DataField("guid")] public int Id { get; set; }

        public bool IsValid { get; set; }

        [DataField("type")] public ItemType type;

        [DataField("icon")] public string icon;

        [DataField("name")] public string name;

        [DataField("desc")] public string[] desc;

        [DataField("sell")] public int sellPrice;

        [DataField("option")] public string[] options;
    }
}