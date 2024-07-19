namespace BackendFunction
{
    public partial class BFunc
    {
        private enum Status
        {
            Hp = 0,
            Atk,
            Critical,
            CriticalAtk,
            AtkSpeed,
            MoveSpeed,
            AtkRange,
            MoveRange,
        }

        private enum ItemType
        {
            Equipment = 0,
            Consumable = 1,
            Ingredient = 2,
            Box = 3,
        }

        private enum EquipmentGrade
        {
            Legend = 0,
            Epic = 1,
            Rare = 2,
            Common = 3
        }

        private enum EquipmentSlot
        {
            Weapon = 0,
            Armor = 1,
            Necklace = 2,
            Ring = 3,
        }

        private enum EquipmentSet
        {
            Executer = 1,
            Slaughterer,
            Punisher,
            Transcendent,
        }   

        private enum ConsumebleType
        {
            ChargeAp = 1,
            Exp = 2,
            Ticket = 3,
            ElementStone = 4,
        }

        private enum IngredientType
        {
            Soul = 1,
            ElemntPeice = 2,
            EquipmentPeice = 3,
        }

        private enum ElementType
        {
            Ground = 1,
            Fire,
            Wind,
            Water,
        }

        private enum ErrorCode
        {
            InvalidRequest,
        }

        private const int PaidDiaID = 1;
        private const int FreeDiaId = 2;
        private const int GoldId = 3;

        private const string DefaultChartId = "129116";

        private const string InfoTable = "info";
        private const string ActTable = "act";
        private const string CurrencyTable = "currency";
        private const string CharactersTable = "characters";
        private const string FormationTable = "formation";
        private const string InventoryTable = "inventory";
    }
}