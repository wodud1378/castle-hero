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
        private string OpenBox(int itemChartId, int statChartId, int boxId)
        {
            var itemChart = LoadChart(itemChartId);
            JsonData boxData = null;
            int index = itemChart.Count - 1;
            // 데이터 상 박스는 뒤에 위치.
            while (index-- >= 0 && boxData == null)
            {
                if (itemChart[index]["guid"].ToInt() != boxId)
                    continue;

                boxData = itemChart[index];
            }

            var random = new Random();
            var ids = boxData["option_1"]
                .ToString()
                .Trim()
                .Split(',')
                .Select(x => int.Parse(x))
                .ToArray();

            var quantities = boxData["option_2"]
                .ToString()
                .Trim()
                .Split(',')
                .Select(x =>
                {
                    var arr = x.Split(':');
                    return random.Next(int.Parse(arr[0]), int.Parse(arr[1]) + 1);
                })
                .ToArray();

            RandomItems(
                itemChart,
                statChartId,
                random,
                ids,
                quantities,
                out int paidDia,
                out int freeDia,
                out int gold,
                out var items
            );

            Action<JsonData> onResult = null;
            var read = new List<TransactionValue>();
            var write = new List<TransactionValue>();
            var data = ReadFromDB(TransactionGet(CurrencyTable, InventoryTable));
            if (paidDia > 0 || freeDia > 0 || gold > 0)
            {
                read.Add(TransactionGetCurrency());
                onResult += json =>
                {
                    var currency = json[CurrencyTable].Cast<Currency>();
                    currency.paidDia += paidDia;
                    currency.freeDia += freeDia;
                    currency.gold += gold;

                    write.Add(TransactionUpdateCurrency(currency));
                };
            }
            if (items.Count > 0)
            {
                read.Add(TransactionGetInventory());
                onResult += (JsonData json) =>
                {
                    var inventory = json[InventoryTable].Cast<Inventory>();
                    AddToInventory(inventory, items);
                };
            }

            WriteToDB(write);

            var result = new OpenBoxResult()
            {
                currency = new Currency{
                    paidDia = paidDia,
                    freeDia = freeDia,
                    gold = gold,
                },
                items = items,
            };

            return result.ToJson();
        }

        private void RandomItems(
            JsonData itemChart,
            JsonData statChart,
            Random random,
            int[] ids,
            int[] quantities,
            out int paidDia,
            out int freeDia,
            out int gold,
            out List<IItem> items
        )
        {
            paidDia = 0;
            freeDia = 0;
            gold = 0;
            items = new List<IItem>();

            int index = 0;
            int count = Math.Min(ids.Length, quantities.Length);
            var filtered = new List<int>();
            while (index++ < count)
            {
                filtered.Clear();
                int id = ids[index];
                int quantity = quantities[index];
                switch (id)
                {
                    case PaidDiaID:
                        paidDia += quantity;
                        break;
                    case FreeDiaId:
                        freeDia += quantity;
                        break;
                    case GoldId:
                        gold += quantity;
                        break;
                    default:
                        filtered.Clear();
                        FilterItemIds(itemChart, id, filtered);
                        for (int i = 0; i < quantity; ++i)
                        {
                            int rand = random.Next(0, filtered.Count - 1);
                            id = filtered[id];

                            var exist = items.Find(x => x.ItemId == id);
                            if (exist == null)
                            {
                                var item = NewItem(itemChart, statChart, random, id, 1);
                                if (item != null)
                                    items.Add(item);
                            }
                            else
                            {
                                exist.Quantity += 1;
                            }
                        }
                        break;
                }
            }
        }

        private void RandomItems(
            JsonData itemChart,
            int statChartId,
            Random random,
            int[] ids,
            int[] quantities,
            out int paidDia,
            out int freeDia,
            out int gold,
            out List<IItem> items
        )
        {
            bool hasEquipItem = false;
            for (int i = 0, count = ids.Length; i < count && !hasEquipItem; ++i)
            {
                hasEquipItem = IsEquipItem(ids[i]);
            }

            var statChart = LoadChart(statChartId);
            RandomItems(
                itemChart,
                statChart,
                random,
                ids,
                quantities,
                out paidDia,
                out freeDia,
                out gold,
                out items
            );
        }

        private void FilterItemIds(JsonData jsonData, int id, List<int> result)
        {
            // a의 각 자릿수
            int length = (int)Math.Log10(id) + 1;

            jsonData.BinarySearch(x =>
            {
                int temp = x["guid"].ToInt();
                if (!MatchesCriteria(id, temp, length))
                    return;

                result.Add(temp);
            });
        }

        // id가 a의 자릿수 조건을 만족하는지 확인하는 함수
        private bool MatchesCriteria(int a, int b, int length)
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

        private IItem NewItem(
            JsonData itemChart,
            JsonData statChart,
            Random random,
            int id,
            int quantity
        )
        {
            string idString = id.ToString();
            var data = itemChart.BinarySerach(x => x["guid"].ToString() == idString);
            if (data == null)
                return null;

            return NewItem(data, statChart, random, quantity);
        }

        private IItem NewItem(JsonData data, int statChartId, Random random, int quantity)
        {
            JsonData statChart = null;
            if (IsEquipItem(data))
                statChart = LoadChart(statChartId);

            return NewItem(data, statChart, random, quantity);
        }

        private IItem NewItem(JsonData data, JsonData statChart, Random random, int quantity)
        {
            var type = (ItemType)data["type"].ToInt();
            return type switch
            {
                ItemType.Equipment => NewEquipItem(data, statChart, random),
                _ => new Item { ItemId = data["guid"].ToInt(), Quantity = quantity }
            };
        }
    }
}
