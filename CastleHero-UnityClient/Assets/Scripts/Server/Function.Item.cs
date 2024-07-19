using LitJson;

namespace BackendFunction
{
    public partial class BFunc
    {
        private bool IsRandomItem(int id) => id % 10 == 0;

        private bool IsEquipItem(int id)
        {
            int val = id / 10000;
            return val >= 1 && val <= 4;
        }

        private bool IsCurrency(int id)
        {
            return id == PaidDiaID || id == FreeDiaId || id == GoldId;
        }

        private bool IsEquipItem(JsonData data) => (ItemType)data["type"].ToInt() == ItemType.Equipment;
    }
}
