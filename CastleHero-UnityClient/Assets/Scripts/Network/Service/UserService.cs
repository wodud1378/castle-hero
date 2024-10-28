using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;
using RGLabs.Utility;

namespace RGLabs.Network.Service
{
    public class UserService : NetworkServiceBase
    {
        public UniTask SaveFormation(FormationDto formation) => UpdateTable(Table.Formation, formation);

        public new UniTask<Result> UpdateStamina() => base.UpdateStamina();

        public UniTask<Result> UpdateNickname(string nickname)
            => Call(onResult => Backend.BMember.UpdateNickname(nickname, onResult.Invoke));
        
        public UniTask<Result<UserDataDto>> GetUserData() => GetTables();
        
        public async UniTask<Result<UserDataDto>> CreateUserData()
        {
            var ids = new List<int>
            {
                10001,
                10006,
                10010,
                10021,
                10023,
                10033,
                10034,
            };

            var units = ids
                .Select(x => UnitGen.NewUnit(x))
                .ToList();
            
            var items = new List<IItem>{
                new Item { ItemId = 52001, Quantity = 1000 },
                new Item { ItemId = 52002, Quantity = 1000 },
                new Item { ItemId = 52003, Quantity = 1000 },
                new Item { ItemId = 61001, Quantity = 860 },
                new Item { ItemId = 61006, Quantity = 860 },
                new Item { ItemId = 61010, Quantity = 860 },
                new Item { ItemId = 61021, Quantity = 860 },
                new Item { ItemId = 61023, Quantity = 860 },
                new Item { ItemId = 61033, Quantity = 860 },
                new Item { ItemId = 61034, Quantity = 860 },
            };

            var response = await Call(onResult => Backend.Chart.GetChartContents(146355.ToString(), onResult.Invoke));
            if (!response.IsSuccess)
                return Result<UserDataDto>.Error(Error.FromServer);

            var defaultData = response.raw.FlattenRows()[0];
            int ap = defaultData["Base_Act"].ToInt();
            int baseCharacterId = defaultData["Base_Character"].ToInt();
            var getTime = await GetServerTime();
            if (!getTime.IsSuccess)
                return Result<UserDataDto>.Error(getTime.error);
            
            var data = new UserDataDto
            {
                stamina = new StaminaDto
                {
                    point = ap,
                    pointLimit = ap,
                    lastUpdate = getTime.data,
                },
                currency = new CurrencyDto
                {
                    gold = 50000000,
                    freeDia = defaultData["Base_Dia"].ToInt(),
                    paidDia = 0,
                },
                characters = new CharactersDto { units = units },
                formation = new FormationDto { fieldUnits = new List<FieldUnit>() },
                inventory = new InventoryDto { items = items },
                gameRecord = new GameRecordDto
                {
                    iconId = baseCharacterId,
                    lastClearedStage = 200,
                    castleLv = 1,
                    dungeon = new List<DungeonRecord>()
                },
                shopRecord = new ShopRecordDto
                {
                    products = new List<Product>(),
                    histories = new List<ShopRecordDto.History>()
                },
            };
            
            void AddInsertQuery(PlayerDataTransactionWrite root, Table table, object obj)
            {
                var name = TableNames[table];
                root.AddInsert(name, ToParam(name, obj));
            }
            
            var write = new PlayerDataTransactionWrite();
            AddInsertQuery(write, Table.Stamina, data.stamina);
            AddInsertQuery(write, Table.Currency, data.currency);
            AddInsertQuery(write, Table.Character, data.characters);
            AddInsertQuery(write, Table.Formation, data.formation);
            AddInsertQuery(write, Table.Inventory, data.inventory);
            AddInsertQuery(write, Table.GameRecord, data.gameRecord);
            AddInsertQuery(write, Table.ShopRecord, data.shopRecord);

            var insert = await Call(onResult =>
                Backend.PlayerData.TransactionWrite(write, onResult.Invoke));

            return insert.IsSuccess 
                ? Result<UserDataDto>.Complete(data) 
                : Result<UserDataDto>.Error(insert.error);
        }
    }
}