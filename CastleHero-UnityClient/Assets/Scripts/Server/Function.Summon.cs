using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using BackEnd;
using LitJson;

namespace BackendFunction
{
    public partial class BFunc
    {
        private string SummonX1(
            int eventIndex,
            int coastIndex,
            string eventChartId,
            string listChartId
        ) => Summon(eventIndex, coastIndex, eventChartId, listChartId, 1);

        private string SummonX10(
            int eventIndex,
            int coastIndex,
            string eventChartId,
            string listChartId
        ) => Summon(eventIndex, coastIndex, eventChartId, listChartId, 10);

        private string Summon(
            int eventIndex,
            int coastIndex,
            string eventChartId,
            string listChartId,
            int count
        )
        {
            var eventChart = LoadChart(eventChartId)[eventIndex];
            var listChart = LoadChart(listChartId);
            var userData = GetUserData();

            Console.WriteLine(JsonMapper.ToJson(userData));

            string countText =
                count == 1
                    ? "Once"
                    : count == 10
                        ? "Tenth"
                        : "??";

            int coastNum = coastIndex + 1;
            string coastTypeKey = $"Summon_Cost_Type_{coastIndex + 1}";
            string coastKey = $"Summon_Cost_{countText}_{coastNum}";

            if (!eventChart.ContainsKey(coastTypeKey) || !eventChart.ContainsKey(coastKey))
                return string.Empty;

            int coastType = eventChart[coastTypeKey].ToInt();
            int coast = eventChart[coastKey].ToInt();
            var data = Backend
                .GameData.TransactionReadV2(
                    TransactionGet(CharactersTable, CurrencyTable, InventoryTable)
                )
                .GetFlattenJSON();

            var currency = data[CurrencyTable].Cast<Currency>();
            var inventory = data[InventoryTable].Cast<Inventory>();
            bool updateCurrency = false;
            switch (coastType)
            {
                case PaidDiaID:
                case FreeDiaId:
                    if (currency.freeDia + currency.paidDia < coast)
                        return string.Empty;

                    ConsumeDia(ref currency.freeDia, ref currency.paidDia, coast);
                    updateCurrency = true;
                    break;
                case GoldId:
                    if (currency.gold < coast)
                        return string.Empty;

                    currency.gold -= coast;
                    updateCurrency = true;
                    break;
                default:
                    var item = inventory.items.Find(x => x.ItemId == coastType);
                    if (item == null || item.Quantity < coast)
                        return string.Empty;

                    break;
            }

            var characters = data[CharactersTable].Cast<Characters>();
            int group = eventChart["Summon_Grp_ID"].ToInt();
            var summonResults = GetSummonResult(
                new Random(),
                listChart,
                characters.units,
                group,
                count
            );

            foreach (var summon in summonResults)
            {
                if (summon is SummonedUnit unit)
                {
                    characters.units.Add(NewUnit(summon.Id));
                }
                else if (summon is SummonedSoul soul)
                {
                    var soulItem = inventory.items.Find(x => x.ItemId == soul.Id);
                    if (soulItem == null)
                    {
                        soulItem = new Item { ItemId = soul.Id, Quantity = 0, };

                        inventory.items.Add(soulItem);
                    }

                    soulItem.Quantity += soul.quantity;
                }
            }

            var write = new List<TransactionValue>
            {
                TransactionUpdateCharacters(characters),
                TransactionUpdateInventory(inventory),
            };

            if (updateCurrency)
                write.Add(TransactionUpdateCurrency(currency));

            WriteToDB(write);

            var result = new SummonResult { summoneds = summonResults };
            return result.ToJson();
        }

        private List<ISummoned> GetSummonResult(
            Random random,
            JsonData listChart,
            List<UnitInfo> exist,
            int group,
            int count
        )
        {
            var result = new List<ISummoned>();
            var weightMap = new Dictionary<ISummoned, float>();
            float totalWeight = 0f;
            foreach (JsonData listItem in listChart)
            {
                if (
                    !int.TryParse(listItem["Summon_Grp_ID"].ToString(), out int grp)
                    && grp != group
                )
                    continue;

                int unitId = listItem["Summon_Grp_Character_ID"].ToInt();
                float weight = listItem["Summon_Grp_Per"].ToFloat();
                ISummoned summon;
                if (exist.Find(x => x.id == unitId) == null)
                {
                    summon = new SummonedUnit { Id = unitId };
                }
                else
                {
                    summon = new SummonedSoul
                    {
                        Id = listItem["Item_Parts_ID"].ToInt(),
                        quantity = listItem["Parts_Value"].ToInt(),
                    };
                }

                weightMap.Add(summon, weight);
                totalWeight += weight;
            }

            for (int i = 0; i < count; ++i)
            {
                float sum = 0f;
                float randomVal = (float)random.NextDouble() * totalWeight;
                using var itr = weightMap.GetEnumerator();
                ISummoned current = null;
                while (sum <= randomVal && itr.MoveNext())
                {
                    current = itr.Current.Key;
                    sum += itr.Current.Value;
                }

                current = current == null ? weightMap.First().Key : current;

                result.Add(current);
            }

            return result;
        }
    }
}
