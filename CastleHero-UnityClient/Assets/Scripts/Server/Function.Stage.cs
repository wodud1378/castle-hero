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
        private string StageClear(
            int stageChartId,
            int levelChartId,
            int itemChartId,
            int statusChartId,
            int stage
        )
        {
            var userData = GetUserData();
            var stageChart = LoadChart(stageChartId);
            var stageData = stageChart[stage - 1];
            var random = new Random();
            int paidDia = 0;
            int freeDia = 0;
            int gold = 0;
            List<IItem> items = null;
            JsonData itemChart = null;
            JsonData statusChart = null;

            var info = userData.info;
            var isFirstClear = info.stage < stage;
            if (isFirstClear)
            {
                int firstRewardId = stageData["Stage_Rwd_First"].ToInt();
                int firstRewardQuantity = stageData["Stage_Rwd_First_Value"].ToInt();
                if (IsCurrency(firstRewardId))
                    itemChart = LoadChart(itemChartId);

                if (IsEquipItem(firstRewardId))
                    statusChart = LoadChart(statusChartId);

                RandomItems(
                    itemChart,
                    statusChart,
                    random,
                    new int[] { firstRewardId },
                    new int[] { firstRewardQuantity },
                    out paidDia,
                    out freeDia,
                    out gold,
                    out items
                );

                info.stage = stage;
                info.focusedStage = stageChart.Count == stage ? stage : stage + 1;
            }
            else
            {
                info.focusedStage = stage;
            }

            if ((float)random.NextDouble() > stageData["Stage_Rwd_Item_Per"].ToFloat())
            {
                int itemId = stageData["Stage_Rwd_Item_ID"].ToInt();
                if (IsCurrency(itemId))
                    itemChart ??= LoadChart(itemChartId);

                if (IsEquipItem(itemId))
                    statusChart ??= LoadChart(statusChartId);

                var item = NewItem(
                    itemChart,
                    statusChart,
                    random,
                    itemId,
                    stageData["Stage_Rwd_Item_Value"].ToInt()
                );
                items.Add(item);
            }

            gold += random.Next(
                stageData["Stage_Rwd_Gold_Min"].ToInt(),
                stageData["Stage_Rwd_Gold_Max"].ToInt() + 1
            );

            var currency = userData.currency;
            currency.paidDia += paidDia;
            currency.freeDia += freeDia;
            currency.gold += gold;

            var write = new List<TransactionValue>
            {
                TransactionUpdateInfo(info),
                TransactionUpdateCurrency(userData.currency),
            };

            if (items.Count > 0)
            {
                var inventory = userData.inventory;
                AddToInventory(inventory, items);

                write.Add(TransactionUpdateInventory(inventory));
            }

            var units = userData
                .formation.fieldUnits.Select(x =>
                    userData.characters.units.Find(unit => unit.id == x.id)
                )
                .ToList();

            int exp = stageData["Stage_Rwd_Exp"].ToInt();
            if (TryLevelUp(levelChartId, units, exp))
            {
                write.Add(TransactionUpdateCharacters(userData.characters));
            }

            WriteToDB(write);

            var result = new StageCleared
            {
                stage = stage,
                exp = exp,
                isFirstClear = isFirstClear,
                currency = currency,
                items = items,
                updated = units,
            };

            return result.ToJson();
        }
    }
}
