using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.DB;
using RGLabs.Utility;

namespace RGLabs.Data.Model
{
    public enum EquipmentGrade
    {
        Legend = 0,
        Epic = 1,
        Rare = 2,
        Common = 3
    }
    
    public enum ConsumeType
    {
        Ap = 1,
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
    }

    public enum ItemType
    {
        Equipment = 0,
        Consumable,
        Ingredient,
        Chest,
    }

    public struct EquipmentOption
    {
        public EquipmentGrade grade;
        public EquipmentSlot slot;
        public int set;
        public int mainStat;
        public List<KeyValuePair<int, float>> setOptions;
    }

    public struct ConsumableOption
    {
        public ConsumeType type;
        public float value;
    }

    public struct IngredientOption
    {
        public IngredientType type;
        public int targetId;
        public int forCombine;
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

        #region Equipment.

        public enum EquipmentOptionIndex
        {
            SetStat = 0,
            SetStatValue = 1,
            Param,
        }
        
        public enum EquipmentParam
        {
            Grade = 0,
            Slot = 1,
            Set = 2,
            MainStat = 3,
        }
        
        public EquipmentOption GetEquipmentOption()
        {
            var option = new EquipmentOption();
            var parameters = options[(int)EquipmentOptionIndex.Param]
                .Trim()
                .Split(',');
            
            for (var param = EquipmentParam.Grade; param <= EquipmentParam.MainStat; ++param)
            {
                int index = (int)param;
                int value = int.Parse(parameters[index]);

                switch (param)
                {
                    case EquipmentParam.Grade: option.grade = (EquipmentGrade)value; break;
                    case EquipmentParam.Slot: option.slot = (EquipmentSlot)value; break;
                    case EquipmentParam.Set: option.set = value; break;
                    case EquipmentParam.MainStat: option.mainStat = value; break;
                }
            }

            option.setOptions = new();

            var types = options[(int)EquipmentOptionIndex.SetStat]
                .Trim()
                .Split(',')
                .Select(int.Parse)
                .ToArray();
            
            var values = options[(int)EquipmentOptionIndex.SetStatValue]
                .Trim()
                .Split(',')
                .Select(float.Parse)
                .ToArray();

            int i = 0;
            while (i.IsValidIndex(types, values))
            {
                option.setOptions.Add(new KeyValuePair<int, float>(types[i], values[i]));
                ++i;
            }

            return option;
        }

        #endregion

        #region Consumable.
        public enum ConsumableOptionIndex
        {
            ConsumeType = 0,
            Value = 1,
        }

        public ConsumableOption GetConsumableOption() =>
            new()
            {
                type = (ConsumeType)int.Parse(options[(int)ConsumableOptionIndex.ConsumeType]),
                value = float.Parse(options[(int)ConsumableOptionIndex.Value])
            };

        #endregion

        #region Ingredient

        public enum IngredientOptionIndex
        {
            TargetId = 0,
            ForCombine = 1,
        }

        public IngredientOption GetIngredientOption() =>
            new()
            {
                type = (IngredientType)((Id - Id / 10000) / 1000),
                targetId = int.Parse(options[(int)IngredientOptionIndex.TargetId]),
                forCombine = int.Parse(options[(int)IngredientOptionIndex.ForCombine])
            };

        #endregion
    }
}