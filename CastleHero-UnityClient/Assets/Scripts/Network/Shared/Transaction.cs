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
            if (tables.Contains(PROFILE_TABLE))
                list.Add(TransactionGetProfile());
            
            if (tables.Contains(ACT_TABLE))
                list.Add(TransactionGetAct());

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

        public static TransactionValue TransactionGetInventory() => TransactionValue.SetGet(INVENTORY_TABLE, new Where());

        public static TransactionValue TransactionGetCharacters() => TransactionValue.SetGet(CHARACTERS_TABLE, new Where());

        public static TransactionValue TransactionGetFormation() => TransactionValue.SetGet(FORMATION_TABLE, new Where());

        public static TransactionValue TransactionGetCurrency() => TransactionValue.SetGet(CURRENCY_TABLE, new Where());

        public static TransactionValue TransactionGetAct() => TransactionValue.SetGet(ACT_TABLE, new Where());
        
        public static TransactionValue TransactionGetProfile() => TransactionValue.SetGet(PROFILE_TABLE, new Where());

        #endregion
    }
}