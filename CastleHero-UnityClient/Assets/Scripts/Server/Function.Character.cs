using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Transactions;
using BackEnd;
using LitJson;

namespace BackendFunction
{
    public partial class BFunc
    {
        #region Via Exp.
        private bool TryLevelUp(int levelChartId, IEnumerable<UnitInfo> units, int expAmount)
        {
            if (units == null)
                return false;

            bool hasUpdate = false;
            var levelChart = LoadChart(levelChartId);
            foreach (var unit in units)
            {
                CalculateLevelUp(levelChart, expAmount, unit.lv, unit.exp, out int lv, out int exp);

                if (unit.lv == lv && unit.exp == exp)
                    continue;

                unit.lv = lv;
                unit.exp = exp;
                hasUpdate = true;
            }

            return hasUpdate;
        }

        private void CalculateLevelUp(
            JsonData levelChart,
            int expAmount,
            int startLv,
            int startExp,
            out int endLv,
            out int endExp
        )
        {
            endLv = startLv;
            endExp = startExp;
            int lastLv = levelChart[^1]["Lv"].ToInt();
            if (startLv == lastLv)
                return;

            endExp += expAmount;

            int count = levelChart.Count;
            for (int i = startLv - 1; i < count; ++i)
            {
                var levelData = levelChart[i];
                int exp = levelData["Exp"].ToInt();
                if (endExp - exp < 0)
                    break;

                endExp -= exp;
                endLv = levelData["Lv"].ToInt() + 1;
            }

            if (endLv == lastLv)
                endExp = 0;
        }
        #endregion

        #region Via Item.
        private string LevelUp(
            int levelChartId,
            int itemChartId,
            int unitId,
            int itemId,
            int itemQty
        )
        {
            var read = TransactionGet(CharactersTable, CurrencyTable, InventoryTable);
            var data = ReadFromDB(read);
            var characters = data[CharactersTable].Cast<Characters>();
            var currency = data[CurrencyTable].Cast<Currency>();
            var inventory = data[InventoryTable].Cast<Inventory>();

            var unit = characters.units.Find(x => x.id == unitId);
            if (unit == null)
                return string.Empty;

            var item = inventory.items.Find(x => x.ItemId == itemId);
            if (item == null || item.Quantity < itemQty)
                return string.Empty;

            var levelChart = LoadChart(levelChartId);
            var itemChart = LoadChart(itemChartId);
            JsonData itemData = null;
            foreach (JsonData temp in itemChart)
            {
                if (temp["Item_Use_ID"].ToInt() == unitId)
                {
                    itemData = temp;
                    break;
                }
            }

            if (itemData == null)
                return string.Empty;

            CalculateLevelUp(
                levelChart,
                itemData,
                itemQty,
                unit.lv,
                unit.exp,
                out int lv,
                out int exp,
                out int gold
            );

            if (currency.gold < gold || (unit.lv == lv && unit.exp == exp))
                return string.Empty;

            currency.gold -= gold;
            item.Quantity -= itemQty;
            unit.lv = lv;
            unit.exp = exp;

            WriteToDB(
                new List<TransactionValue>
                {
                    TransactionUpdateCurrency(currency),
                    TransactionUpdateCharacters(characters),
                    TransactionUpdateInventory(inventory),
                }
            );

            var result = new GrowthResult
            {
                unit = unit,
                leftCurrency = currency,
                leftItem = item,
            };

            return result.ToJson();
        }

        private void CalculateLevelUp(
            JsonData levelChart,
            JsonData item,
            int quantity,
            int startLv,
            int startExp,
            out int endLv,
            out int endExp,
            out int requireGold
        )
        {
            endLv = startLv;
            requireGold = 0;

            endExp = quantity * item["Item_Use_Option_Value"].ToInt();
            int count = levelChart.Count;
            for (int i = startLv - 1; i < count; ++i)
            {
                var levelData = levelChart[i];
                int exp = levelData["Exp"].ToInt();
                if (endExp - exp < 0)
                    break;

                endExp -= exp;
                endLv = levelData["Lv"].ToInt() + 1;
                requireGold += levelData["Gold"].ToInt();
            }

            if (endLv == levelChart[^1]["Lv"].ToInt())
                endExp = 0;
        }

        private string Upgrade(
            int rateChartId,
            int itemChartId,
            int unitId,
            int itemId,
            int itemQty
        )
        {
            var read = TransactionGet(CharactersTable, CurrencyTable, InventoryTable);
            var data = ReadFromDB(read);
            var characters = data[CharactersTable].Cast<Characters>();
            var currency = data[CurrencyTable].Cast<Currency>();
            var inventory = data[InventoryTable].Cast<Inventory>();

            var unit = characters.units.Find(x => x.id == unitId);
            if (unit == null)
                return string.Empty;

            var item = inventory.items.Find(x => x.ItemId == itemId);
            if (item == null || item.Quantity < itemQty)
                return string.Empty;

            var rateChart = LoadChart(rateChartId);
            var itemChart = LoadChart(itemChartId);
            JsonData itemData = null;
            foreach (JsonData temp in itemChart)
            {
                if (temp["Item_Use_ID"].ToInt() == unitId)
                {
                    itemData = temp;
                    break;
                }
            }

            CalculateUpgrade(
                rateChart,
                itemChart,
                item.Quantity,
                unit.rate,
                out int rate,
                out int gold
            );

            if (currency.gold < gold || unit.rate == rate)
                return string.Empty;

            currency.gold -= gold;
            item.Quantity -= itemQty;
            unit.rate = rate;

            WriteToDB(
                new List<TransactionValue>
                {
                    TransactionUpdateCurrency(currency),
                    TransactionUpdateCharacters(characters),
                    TransactionUpdateInventory(inventory),
                }
            );

            var result = new GrowthResult
            {
                unit = unit,
                leftCurrency = currency,
                leftItem = item,
            };

            return result.ToJson();
        }

        private void CalculateUpgrade(
            JsonData rateChart,
            JsonData item,
            int quantity,
            int startRate,
            out int endRate,
            out int requireGold
        )
        {
            endRate = startRate;
            requireGold = 0;

            int count = rateChart.Count;
            for (int i = startRate - 1; i < count; ++i)
            {
                var rateData = rateChart[i];
                int requireSoul = rateData["Rate_Soul"].ToInt();
                if (quantity - requireSoul < 0)
                    break;

                quantity -= requireSoul;
                endRate = rateData["Rate_Lv"].ToInt() + 1;
                requireGold += rateData["Rate_Gold_Normal"].ToInt();
            }
        }
        #endregion

        private string Equip(int unitId, string guid)
        {
            var read = TransactionGet(CharactersTable, InventoryTable);
            var data = ReadFromDB(read);
            var characters = data[CharactersTable].Cast<Characters>();
            var inventory = data[InventoryTable].Cast<Inventory>();

            var unit = characters.units.Find(x => x.id == unitId);
            if (unit == null)
                return ReturnInvalidRequest();

            var item = inventory.items.Find(x =>
                x is EquipItem equipItem && equipItem.Guid == guid
            );

            if (item == null)
                return ReturnInvalidRequest();

            if (unit.equipments.Contains(guid))
                return ReturnInvalidRequest();

            var current = (EquipItem)item;
            var exist = inventory
                .items.OfType<EquipItem>()
                .Where(x => unit.equipments.Contains(x.Guid))
                .FirstOrDefault(x => x.slot == current.slot);

            if (exist != null)
            {
                exist.character = 0;
                unit.equipments.Remove(exist.Guid);
            }

            unit.equipments.Add(current.Guid);

            WriteToDB(
                new List<TransactionValue>
                {
                    TransactionUpdateCharacters(characters),
                    TransactionUpdateInventory(inventory),
                }
            );

            var result = new UnitResult { unit = unit };

            return result.ToJson();
        }

        private UnitInfo NewUnit(int id) => NewUnit(id, 1, 0);

        private UnitInfo NewUnit(int id, int lv, int rate)
        {
            return new UnitInfo()
            {
                id = id,
                lv = lv,
                rate = rate,
                equipments = new List<string>(),
            };
        }
    }
}
