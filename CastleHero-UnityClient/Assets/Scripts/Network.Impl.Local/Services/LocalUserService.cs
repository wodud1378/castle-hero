using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;

namespace CastleHero.Network.Impl.Local.Services
{
    public class LocalUserService : LocalNetworkServiceBase, IUserService
    {
        private string _nickname = "Player";

        public LocalUserService(LocalUserDataStore store) : base(store) { }

        public UniTask SaveFormation(FormationDto formation)
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.CompletedTask;

            get.data.formation = formation;
            Save(get.data);
            return UniTask.CompletedTask;
        }

        public UniTask<Result> UpdateStamina()
        {
            var get = Get();
            if (!get.IsSuccess)
                return UniTask.FromResult(Result.Error(get.error));

            RecoverStamina(get.data.stamina);
            Save(get.data);
            return UniTask.FromResult(Result.Complete());
        }

        public UniTask<Result> UpdateNickname(string nickname)
        {
            _nickname = nickname;
            return UniTask.FromResult(Result.Complete());
        }

        public UniTask<Result<UserDataDto>> GetUserData()
        {
            var data = Store.Load();
            return UniTask.FromResult(data != null
                ? Result<UserDataDto>.Complete(data)
                : Result<UserDataDto>.Error(Error.DataNotFound, 404));
        }

        public UniTask<Result<UserDataDto>> CreateUserData()
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

            var items = new List<IItem>
            {
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

            const int defaultAp = 100;
            const int defaultDia = 1000;
            const int defaultIcon = 10001;

            var data = new UserDataDto
            {
                stamina = new StaminaDto
                {
                    point = defaultAp,
                    pointLimit = defaultAp,
                    lastUpdate = ServerTime.Now,
                },
                currency = new CurrencyDto
                {
                    gold = 50000000,
                    freeDia = defaultDia,
                    paidDia = 0,
                },
                characters = new CharactersDto { units = units },
                formation = new FormationDto { fieldUnits = new List<FieldUnit>() },
                inventory = new InventoryDto { items = items },
                gameRecord = new GameRecordDto
                {
                    iconId = defaultIcon,
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

            Save(data);
            return UniTask.FromResult(Result<UserDataDto>.Complete(data));
        }
    }
}
