using RGLabs.Data.DB;

namespace RGLabs.Data.Model
{
    public enum ItemTypeCode
    {
        Equipment = 10000,
        Consumable = 50000,
        Ingredient = 60000,
        Chest = 70000,
    }

    public enum EquipmentGradeCode
    {
        Legend = 1000,
        Epic = 2000,
        Rare = 3000,
        Common = 4000
    }

    public enum IngredientGradeCode
    {
        Legend = 1000,
        Epic = 2000,
        Rare = 3000,
        High = 4000,
        Middle = 5000,
        Low = 6000,
    }

    public enum ChestTypeCode
    {
        Soul = 1000,
        Ap = 2000,
        Exp = 3000,
        ElementalStone = 4000,
        Equipment = 5000,
        Gold = 6000,
        General = 7000,
    }
    
    public interface IItemEntity : IEntity
    {
        public ItemTypeCode TypeCode { get; }
        
        public string Icon { get; }
        public string Name { get; }
        public string Desc { get; }
    }

    public struct EquipmentEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Equipment;
        
        [DataField("Item_Equip_ID")]
        public int Id { get; set; }
        
        [DataField("Item_Equip_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Equip_Name")]
        public string Name { get; set; }

        [DataField("Item_Equip_Subject")]
        public string Desc { get; set; }

        [DataField("Item_Equip_Class")] 
        public int grade;

        [DataField("Item_Equip_Slot")]
        public int slot;

        [DataField("Item_Equip_Set")]
        public string set;

        [DataField("Item_Equip_Set_Type")] 
        public int[] setOptionStats;

        [DataField("Item_Equip_Set_Value")] 
        public float[] setOptionValues;

        [DataField("Item_Equip_Sell")]
        public int sellPrice;
    }
    
    public struct ConsumableEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Consumable;
        
        [DataField("Item_Use_ID")]
        public int Id { get; set; }
        
        [DataField("Item_Use_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Use_Name")]
        public string Name { get; set; }

        [DataField("Item_Use_Subject")]
        public string Desc { get; set; }
        
        [DataField("Item_Use_Option")] 
        public int option;

        [DataField("Item_Use_Option_Value")]
        public int optionValue;
    }

    public struct IngredientEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Ingredient;
        
        [DataField("Item_Parts_ID")]
        public int Id { get; set; }
        
        [DataField("Item_Parts_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Parts_Name")]
        public string Name { get; set; }

        [DataField("Item_Parts_Subject")]
        public string Desc { get; set; }

        [DataField("Item_Parts_Combine")]
        public int forCombine;
        
        [DataField("Item_Parts_Gain")]
        public int resultItemId;
        
        [DataField("Item_Parts_Sell")]
        public int sellPrice;
    }
    
    public struct ChestEntity : IItemEntity
    {
        public ItemTypeCode TypeCode => ItemTypeCode.Chest;
        
        [DataField("Item_Chest_ID")]
        public int Id { get; set; }
        
        [DataField("Item_Chest_Icon")]
        public string Icon { get; set; }

        [DataField("Item_Chest_Name")]
        public string Name { get; set; }

        [DataField("Item_Chest_Subject")]
        public string Desc { get; set; }

        [DataField("Item_Chest_Type")]
        public int type;
        
        [DataField("Item_Chest_Value_Min")]
        public int minQty;
        
        [DataField("Item_Chest_Value_Max")]
        public int maxQty;

        [DataField("Item_Chest_Item_Value")]
        public int resultItemId;
    }
}