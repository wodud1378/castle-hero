using System.Collections.Generic;
using BackEnd;

namespace BackendFunction
{
    public partial class BFunc
    {
        private string AddItems(int[] itemIds, int[] quantities)
        {
            var result = ReadFromDB(TransactionGet(InventoryTable));
            var inventory = result.Cast<Inventory>();

            int index = 0;
            var items = new List<IItem>();
            while(index < itemIds.Length && index < quantities.Length)
            {
                items.Add(new Item {
                    ItemId = itemIds[index],
                    Quantity = quantities[index],
                });
            }

            AddToInventory(inventory, items);
            WriteToDB(new List<TransactionValue> { TransactionUpdateInventory(inventory) });

            return inventory.ToJson();
        }

        private string AddCurrency(int paidDia, int freeDia, int gold)
        {
            var result = ReadFromDB(TransactionGet(CurrencyTable));
            var currency = result.Cast<Currency>();

            currency.paidDia += paidDia;
            currency.freeDia += freeDia;
            currency.gold += gold;

            WriteToDB(new List<TransactionValue> { TransactionUpdateCurrency(currency) });

            return currency.ToJson();
        }
    }
}
