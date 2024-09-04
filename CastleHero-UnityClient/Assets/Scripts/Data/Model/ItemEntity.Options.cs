using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using RGLabs.Unit.Components;
using RGLabs.Utility;

namespace RGLabs.Data.Model
{
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
    
    public struct ElementalOption
    {
        public Elemental.Type type;
        public int lv;
    }

    public struct IngredientOption
    {
        public IngredientType type;
        public int targetId;
        public int forCombine;
    }
    
    public struct ChestOption
    {
        public List<KeyValuePair<int, List<int>>> itemMap;
        public int[] Ids => itemMap.Select(x => x.Key).ToArray();
        public int[] min;
        public int[] max;
    }

    [StructLayout(LayoutKind.Auto)]
    public partial struct ItemEntity
    {
        public EquipmentOption optionEquip => GetEquipmentOption();
        public ConsumableOption optionConsume => GetConsumableOption();
        public IngredientOption optionIngredient => GetIngredientOption();
        public ChestOption optionChest => GetChestOption(false);
        public ChestOption optionChestFull => GetChestOption();

        private enum EquipmentOptionIndex
        {
            SetStat = 0,
            SetStatValue = 1,
            Param,
        }

        private enum EquipmentParam
        {
            Grade = 0,
            Slot = 1,
            Set = 2,
            MainStat = 3,
        }

        private EquipmentOption GetEquipmentOption()
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
                    case EquipmentParam.Grade:
                        option.grade = (EquipmentGrade)value;
                        break;
                    case EquipmentParam.Slot:
                        option.slot = (EquipmentSlot)value;
                        break;
                    case EquipmentParam.Set:
                        option.set = value;
                        break;
                    case EquipmentParam.MainStat:
                        option.mainStat = value;
                        break;
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

        
        private ConsumableOption GetConsumableOption() =>
            new()
            {
                type = (ConsumeType)int.Parse(options[0]),
                value = float.Parse(options[1])
            };

        public bool TryGetElementalOption(out ElementalOption option)
        {
            if ((ConsumeType)int.Parse(options[0]) != ConsumeType.ElementalStone)
            {
                option = default;
                return false;
            }
            
            option = new ElementalOption
            {
                type = (Elemental.Type)int.Parse(options[1]),
                lv = int.Parse(options[2])
            };
            return true;
        }

        private IngredientOption GetIngredientOption() =>
            new()
            {
                type = (IngredientType)((Id - Id / 10000) / 1000),
                targetId = int.Parse(options[0]),
                forCombine = int.Parse(options[1])
            };
        
        private ChestOption GetChestOption(bool full = true)
        {
            string[] ToOptionArray(string optionString)
            {
                return optionString
                    .Trim()
                    .Replace("[", string.Empty)
                    .Replace("]", string.Empty)
                    .Split(',');
            }

            var quantities = ToOptionArray(options[1])
                .Select(x => x.Split(':')
                    .Select(int.Parse)
                    .ToArray())
                .ToList();
            
            var option = new ChestOption
            {
                itemMap = new(),
                min = quantities.Select(x => x[0]).ToArray(),
                max = quantities.Select(x => x[1]).ToArray(),
            };
            
            var ids = ToOptionArray(options[0]).Select(int.Parse).ToArray();
            if (full)
            {
                foreach (var id in ids)
                {
                    var related = id.GetRelatedItemIds();
                    option.itemMap.Add(new(id, related));
                }
            }
            else
            {
                foreach (var id in ids)
                {
                    option.itemMap.Add(new(id, null));
                }
            }

            return option;
        }
    }
}