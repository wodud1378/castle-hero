using System.Collections.Generic;
using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public enum ItemTypeCode
    {
        Equipment = 0,
        Consumable = 5,
        Ingredient = 6,
        Chest = 7,
    }

    public enum EquipmentGradeCode
    {
        Legend = 0,
        Epic = 1,
        Rare = 2,
        Common = 3
    }

    public enum IngredientGradeCode
    {
        Legend = 1,
        Epic = 2,
        Rare = 3,
        High = 4,
        Middle = 5,
        Low = 6,
    }

    public enum ChestTypeCode
    {
        Soul = 1,
        Ap = 2,
        Exp = 3,
        ElementalStone = 4,
        Equipment = 5,
        Gold = 6,
        General = 7,
    }

    public enum ConsumeOption
    {
        Soul = 0,
        Ap = 1,
        Exp = 2,
        PlayTicket = 3,
        SummonTicket = 4,
        ElementalStone = 5,
    }

    public enum EquipmentSlot
    {
        Weapon = 0,
        Armor = 1,
        Ring = 2,
        Necklace = 3,
    }
    
    public interface IItemEntity : IEntity
    {
        public ItemTypeCode TypeCode { get; }
        
        public string Icon { get; }
        public string Name { get; }
        public string Desc { get; }
        public int SellPrice { get; }
    }

    public struct EquipmentEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Equipment;
        
        [DataField("Item_Equip_ID")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }
        
        [DataField("Item_Equip_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Equip_Name")]
        public string Name { get; set; }

        [DataField("Item_Equip_Subject")]
        public string Desc { get; set; }
        
        [DataField("Item_Equip_Sell")]
        public int SellPrice { get; set; }

        [DataField("Item_Equip_Class")] 
        public EquipmentGradeCode grade;

        [DataField("Item_Equip_Slot")]
        public EquipmentSlot slot;

        [DataField("Item_Equip_Set")]
        public string set;

        [DataField("Item_Equip_Set_Type")] 
        public int[] setOptionStats;

        [DataField("Item_Equip_Set_Value")] 
        public float[] setOptionValues;
    }
    
    public struct ConsumableEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Consumable;
        
        [DataField("Item_Use_ID")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }
        
        [DataField("Item_Use_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Use_Name")]
        public string Name { get; set; }

        [DataField("Item_Use_Subject")]
        public string Desc { get; set; }
        
        public int SellPrice { get; set; }
        
        [DataField("Item_Use_Option")] 
        public ConsumeOption option;

        [DataField("Item_Use_Option_Value")]
        public int optionValue;

        [DataField("Item_Use_Effect_Subject")]
        public string consumeDesc;
    }

    public struct IngredientEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Ingredient;
        
        [DataField("Item_Parts_ID")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }
        
        [DataField("Item_Parts_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Parts_Name")]
        public string Name { get; set; }

        [DataField("Item_Parts_Subject")]
        public string Desc { get; set; }
        
        [DataField("Item_Parts_Sell")]
        public int SellPrice { get; set; }

        [DataField("Item_Parts_Combine")]
        public int forCombine;
        
        [DataField("Item_Parts_Gain")]
        public int resultItemId;
    }
    
    public struct ChestEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Chest;
        
        [DataField("Item_Chest_ID")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }
        
        [DataField("Item_Chest_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Chest_Name")]
        public string Name { get; set; }

        [DataField("Item_Chest_Subject")]
        public string Desc { get; set; }
        
        public int SellPrice { get; set; }

        [DataField("Item_Chest_Type")]
        public int type;
        
        [DataField("Item_Chest_Value_Min")]
        public int minQty;
        
        [DataField("Item_Chest_Value_Max")]
        public int maxQty;

        [DataField("Item_Chest_Item_Value")]
        public int resultItemId;
    }

    public enum ItemType
    {
        Equipment = 0,
        Consumable,
        Ingredient,
        Chest,
    }

    public struct ItemEntity : IEntity
    {
        [DataField("id")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("type")]
        public ItemType type;
        
        [DataField("icon")]
        public string icon;
        
        [DataField("Item_Name")] 
        public string name;
        
        [DataField("Item_Desc")]
        public string[] desc;
        
        [DataField("Item_Sell")]
        public int sellPrice;
        
        [DataField("Item_Option")]
        public string[] options;
    }
}