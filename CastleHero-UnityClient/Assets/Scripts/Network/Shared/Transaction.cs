using System.Collections.Generic;
using System.Linq;
using BackEnd;

namespace RGLabs.Network
{
    public static partial class BackendWrapper
    {
        private static List<TransactionValue> TransactionGet(params string[] tables)
        {
            var list = new List<TransactionValue>();
            if (tables.Contains(InfoTable))
                list.Add(TransactionGetInfo());

            if (tables.Contains(CurrencyTable))
                list.Add(TransactionGetInfo());

            if (tables.Contains(CharactersTable))
                list.Add(TransactionGetInfo());

            if (tables.Contains(FormationTable))
                list.Add(TransactionGetInfo());

            if (tables.Contains(InventoryTable))
                list.Add(TransactionGetInfo());

            return list;
        }

        #region Get.

        private static TransactionValue TransactionGetInventory()
        {
            return TransactionValue.SetGet(InventoryTable, new Where());
        }

        private static TransactionValue TransactionGetCharacters()
        {
            return TransactionValue.SetGet(CharactersTable, new Where());
        }

        private static TransactionValue TransactionGetFormation()
        {
            return TransactionValue.SetGet(FormationTable, new Where());
        }

        private static TransactionValue TransactionGetCurrency()
        {
            return TransactionValue.SetGet(CurrencyTable, new Where());
        }

        private static TransactionValue TransactionGetInfo()
        {
            return TransactionValue.SetGet(InfoTable, new Where());
        }
        #endregion
    }
}