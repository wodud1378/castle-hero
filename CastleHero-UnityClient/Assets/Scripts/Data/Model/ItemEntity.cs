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

    public struct ChestOption
    {
        public int[] items;
    }

    public struct ItemEntity : IEntity
    {
        [DataField("guid")]
        public int Id { get; set; }
        
        public bool IsValid { get; set; }

        [DataField("type")]
        public ItemType type;
        
        [DataField("icon")]
        public string icon;
        
        [DataField("name")] 
        public string name;
        
        [DataField("desc")]
        public string[] desc;
        
        [DataField("sell")]
        public int sellPrice;
        
        [DataField("option")]
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

        #region Chest

        public ChestOption GetChestOption()
        {
            void FilterItemIds(int id, List<int> result)
            {
                // a의 각 자릿수
                int length = (int)Math.Log10(id) + 1;
                var db = Storage.db.items;
                db.ForEach(x =>
                {
                    if (!MatchesCriteria(id, x.Id, length))
                        return;
                    
                    if(!result.Contains(x.Id))
                        result.Add(x.Id);
                });
            }
            
            bool MatchesCriteria(int a, int b, int length)
            {
                for (int i = 0; i < length; i++)
                {
                    int aDigit = a / (int)Math.Pow(10, length - i - 1) % 10;
                    int idDigit = b / (int)Math.Pow(10, length - i - 1) % 10;

                    if (aDigit != 0 && aDigit != idDigit)
                    {
                        return false;
                    }
                }
                return true;
            }

            var ids = options[0]
                .Trim()
                .Replace("[", string.Empty)
                .Replace("]", string.Empty)
                .Split(',')
                .Select(int.Parse);

            var list = new List<int>();

            foreach (var id in ids)
            {
                FilterItemIds(id, list);
            }

            return new() { items = list.ToArray() };
        }

        #endregion
    }
}