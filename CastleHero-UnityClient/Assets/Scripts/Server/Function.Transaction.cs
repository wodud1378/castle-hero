using System.Collections.Generic;
using System.Linq;
using BackEnd;
using LitJson;
using Newtonsoft.Json;

namespace BackendFunction
{
    public partial class BFunc
    {
        private const string DataFieldName = "data";

        private JsonData ReadFromDB(List<TransactionValue> transactions)
            => Backend.GameData.TransactionReadV2(transactions).GetFlattenJSON();

        private BackendReturnObject WriteToDB(List<TransactionValue> transactions)
            => Backend.GameData.TransactionWriteV2(transactions);

        private List<TransactionValue> TransactionGet(params string[] tables)
        {
            var list = new List<TransactionValue>();
            if (tables.Contains(InfoTable))
                list.Add(TransactionGetInfo());

            if (tables.Contains(CurrencyTable))
                list.Add(TransactionGetCurrency());

            if (tables.Contains(CharactersTable))
                list.Add(TransactionGetCharacters());

            if (tables.Contains(FormationTable))
                list.Add(TransactionGetFormation());

            if (tables.Contains(InventoryTable))
                list.Add(TransactionGetInventory());

            return list;
        }

        #region Get.
        private TransactionValue TransactionGetInventory() => TransactionValue.SetGet(InventoryTable, new Where());

        private TransactionValue TransactionGetCharacters() => TransactionValue.SetGet(CharactersTable, new Where());

        private TransactionValue TransactionGetFormation() => TransactionValue.SetGet(FormationTable, new Where());

        private TransactionValue TransactionGetCurrency() => TransactionValue.SetGet(CurrencyTable, new Where());

        private TransactionValue TransactionGetAct() => TransactionValue.SetGet(ActTable, new Where());

        private TransactionValue TransactionGetInfo() => TransactionValue.SetGet(InfoTable, new Where());
        #endregion

        #region Insert.
        private TransactionValue TransactionInsertInventory(Inventory inventory)
        {
            return TransactionValue.SetInsert(
                InventoryTable,
                new Param() { { DataFieldName, inventory.ToJson() }  }
            );
        }

        private TransactionValue TransactionInsertCharacters(Characters characters)
        {
            return TransactionValue.SetInsert(
                CharactersTable,
                new Param() { { DataFieldName, characters.ToJson() }, }
            );
        }

        private TransactionValue TransactionInsertFormation(Formation formation)
        {
            return TransactionValue.SetInsert(
                FormationTable,
                new Param() { { DataFieldName, formation.ToJson() } }
            );
        }

        private TransactionValue TransactionInsertCurrency(Currency currency)
        {
            return TransactionValue.SetInsert(
                CurrencyTable,
                new Param() { { DataFieldName, currency.ToJson() },}
            );
        }

        private TransactionValue TransactionInsertAct(Act act)
        {
            return TransactionValue.SetInsert(
                ActTable,
                new Param() { DataFieldName, act.ToJson() }
            );
        }

        private TransactionValue TransactionInsertInfo(Info info)
        {
            return TransactionValue.SetInsert(
                InfoTable,
                new Param() { { DataFieldName, info.ToJson() } }
            );
        }
        #endregion

        #region Update.
        private TransactionValue TransactionUpdateInventory(Inventory inventory)
        {
            return TransactionValue.SetUpdate(
                InventoryTable,
                new Where(),
                new Param() { { DataFieldName, inventory.ToJson() }  }
            );
        }

        private TransactionValue TransactionUpdateCharacters(Characters characters)
        {
            return TransactionValue.SetUpdate(
                CharactersTable,
                new Where(),
                new Param() { { DataFieldName, characters.ToJson() }, }
            );
        }

        private TransactionValue TransactionUpdateFormation(Formation formation)
        {
            return TransactionValue.SetUpdate(
                FormationTable,
                new Where(),
                new Param() { { DataFieldName, formation.ToJson() } }
            );
        }

        private TransactionValue TransactionUpdateCurrency(Currency currency)
        {
            return TransactionValue.SetUpdate(
                CurrencyTable,
                new Where(),
                new Param() { { DataFieldName, currency.ToJson() },}
            );
        }

        private TransactionValue TransactionUpdateAct(Act act)
        {
            return TransactionValue.SetUpdate(
                InfoTable,
                new Where(),
                new Param() { { DataFieldName, act.ToJson() } }
            );
        }

        private TransactionValue TransactionUpdateInfo(Info info)
        {
            return TransactionValue.SetUpdate(
                InfoTable,
                new Where(),
                new Param() { { DataFieldName, info.ToJson() } }
            );
        }
        #endregion
    }
}
