using System;
using System.Collections.Generic;
using LitJson;

namespace BackendFunction
{
    public partial class BFunc
    {
        private EquipItem NewEquipItem(JsonData itemData, JsonData statusChart, Random random)
        {
            var parameters = itemData["option_3"].ToString().Split(',');
            int grade = parameters[0].ToInt();
            int slot = parameters[1].ToInt();
            string minValKey = $"Equip_Status_Main_Min_{grade}";
            string maxValKey = $"Equip_Status_Main_Max_{grade}";
            int mainStatId = parameters[4].ToInt();
            var main =
                mainStatId != -1
                    ? NewStat(statusChart, random, mainStatId, minValKey, maxValKey)
                    : NewStat(
                        statusChart[random.Next(0, statusChart.Count)],
                        random,
                        minValKey,
                        maxValKey
                    );

            int subStatCount = (EquipmentGrade)grade switch
            {
                EquipmentGrade.Legend => 8,
                EquipmentGrade.Epic => 6,
                EquipmentGrade.Rare => 4,
                EquipmentGrade.Common => 2,
                _ => 0,
            };

            var sub = new List<EquipItem.Stat>();
            minValKey = $"Equip_Status_Sub_Min_{grade}";
            maxValKey = $"Equip_Status_Sub_Max_{grade}";
            for (int i = 0; i < subStatCount; ++i)
            {
                sub.Add(
                    NewStat(
                        statusChart[random.Next(0, statusChart.Count)],
                        random,
                        minValKey,
                        maxValKey
                    )
                );
            }

            return new EquipItem
            {
                Guid = Guid.NewGuid().ToString(),
                main = main,
                sub = sub,
                slot = slot,
                Quantity = 1,
                element = new EquipItem.Element { type = 0, lv = 0, }
            };
        }

        private EquipItem.Stat NewStat(
            JsonData chart,
            Random random,
            int status,
            string minKey,
            string maxKey
        )
        {
            JsonData data = null;
            int index = 0;
            int count = chart.Count;
            while (index++ < count && data != null)
            {
                var temp = chart[index];
                if (temp["Equip_Status"].ToInt() != status)
                    continue;

                data = temp;
            }

            return NewStat(data, random, minKey, maxKey);
        }

        private EquipItem.Stat NewStat(JsonData data, Random random, string minKey, string maxKey)
        {
            int status = data["Equip_Status"].ToInt();
            return new EquipItem.Stat
            {
                type = status,
                value = (Status)status switch
                {
                    Status.Hp => random.Next(data[minKey].ToInt(), data[minKey].ToInt() + 1),
                    Status.Atk => random.Next(data[minKey].ToInt(), data[minKey].ToInt() + 1),
                    _
                        => ((float)random.NextDouble())
                            * (data[maxKey].ToFloat() - data[minKey].ToFloat())
                            + data[minKey].ToFloat()
                }
            };
        }
    }
}
