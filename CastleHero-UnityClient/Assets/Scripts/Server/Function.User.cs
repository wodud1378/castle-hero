using System;
using System.Collections.Generic;
using System.IO;
using BackEnd;
using LitJson;

namespace BackendFunction
{
    public partial class BFunc
    {
        private void AddToInventory(Inventory inventory, List<IItem> items)
        {
            var inventoryItems = inventory.items;
            foreach (var item in items)
            {
                var exist =
                    item is EquipItem ? null : inventoryItems.Find(x => x.ItemId == item.ItemId);

                if (exist == null)
                    inventoryItems.Add(item);
                else
                    exist.Quantity += item.Quantity;
            }
        }

        private string SetDefaultUserData()
        {
            var defaultData = LoadChart(DefaultChartId)[0];
            var userData = DefaultUserData(defaultData);

            WriteToDB(new List<TransactionValue> {
                TransactionInsertInfo(userData.info),
                TransactionInsertAct(userData.act),
                TransactionInsertCurrency(userData.currency),
                TransactionInsertCharacters(userData.characters),
                TransactionInsertFormation(userData.formation),
                TransactionInsertInventory(userData.inventory)
            });

            return userData.ToJson();
        }

        private UserData DefaultUserData(JsonData defaultData)
        {
            return new UserData
            {
                info = new Info
                {
                    stage = 1,
                    focusedStage = 1,
                    castleLv = 1,
                },
                act = new Act
                {
                    point = defaultData["Base_Act"].ToInt(),
                    pointLimit = 100,
                    lastUsage = DateTime.Now,
                },
                currency = new Currency
                {
                    gold = defaultData["Base_Gold"].ToInt(),
                    freeDia = defaultData["Base_Dia"].ToInt(),
                    paidDia = 0,
                },
                characters = new Characters
                {
                    units = new List<UnitInfo> { NewUnit(defaultData["Base_Character"].ToInt()) }
                },
                formation = new Formation { fieldUnits = new List<FieldUnit>() },
                inventory = new Inventory { items = new List<IItem>() }
            };
        }

        private UserData GetUserData()
        {
            var read = TransactionGet(
                InfoTable,
                ActTable,
                CurrencyTable,
                CharactersTable,
                FormationTable,
                InventoryTable
            );
            var result = ReadFromDB(read);
            return new UserData
            {
                info = result[InfoTable].Cast<Info>(),
                act = result[ActTable].Cast<Act>(),
                currency = result[CurrencyTable].Cast<Currency>(),
                characters = result[CharactersTable].Cast<Characters>(),
                formation = result[FormationTable].Cast<Formation>(),
                inventory = result[InfoTable].Cast<Inventory>(),
            };
        }

        private BackendReturnObject SaveUserData(UserData userData)
        {
            return WriteToDB(
                new List<TransactionValue>
                {
                    TransactionUpdateInfo(userData.info),
                    TransactionUpdateCurrency(userData.currency),
                    TransactionUpdateCharacters(userData.characters),
                    TransactionUpdateFormation(userData.formation),
                    TransactionUpdateInventory(userData.inventory),
                }
            );
        }
    }
}
