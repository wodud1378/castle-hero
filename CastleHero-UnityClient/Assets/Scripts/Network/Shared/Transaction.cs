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
            if (tables.Contains(INFO_TABLE))
                list.Add(TransactionGetInfo());

            if (tables.Contains(CURRENCY_TABLE))
                list.Add(TransactionGetCurrency());

            if (tables.Contains(CHARACTERS_TABLE))
                list.Add(TransactionGetCharacters());

            if (tables.Contains(FORMATION_TABLE))
                list.Add(TransactionGetFormation());

            if (tables.Contains(INVENTORY_TABLE))
                list.Add(TransactionGetInventory());

            return list;
        }

        #region Get.

        public static TransactionValue TransactionGetInventory()
        {
            return TransactionValue.SetGet(INVENTORY_TABLE, new Where());
        }

        public static TransactionValue TransactionGetCharacters()
        {
            return TransactionValue.SetGet(CHARACTERS_TABLE, new Where());
        }

        public static TransactionValue TransactionGetFormation()
        {
            return TransactionValue.SetGet(FORMATION_TABLE, new Where());
        }

        public static TransactionValue TransactionGetCurrency()
        {
            return TransactionValue.SetGet(CURRENCY_TABLE, new Where());
        }

        public static TransactionValue TransactionGetInfo()
        {
            return TransactionValue.SetGet(INFO_TABLE, new Where());
        }
        #endregion
    }
}