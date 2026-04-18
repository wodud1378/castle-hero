using Cysharp.Threading.Tasks;
using CastleHero.Common.Localize;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Boot;
using CastleHero.Network.Shared;
using LitJson;
using UnityEngine;

namespace CastleHero.Network.Impl.Local.Boot
{
    public class LocalBootService : IBootService
    {
        private readonly IServiceLocator _sl;
        private readonly IBootServiceHandler _handler;

        public LocalBootService(IBootServiceHandler handler)
        {
            _sl = ServiceLocator.Instance;
            _handler = handler;
        }

        public async UniTask Start()
        {
            Debug.Log("[Local] 부팅 시작");

            Init();

            var userData = await LoadOrCreateUserData();

            await InitChart("Player", userData);

            _handler.OnInitDone();

            Debug.Log("[Local] 부팅 성공");
        }

        private void Init()
        {
            _sl.Register(new EntranceHolder());
            _sl.Register(new SettingRepository());

            var soundPath = Resources.Load<SoundPath>("Sound/SoundPath");
            if (soundPath != null)
                _sl.Register(soundPath);

            var localize = new LocalizeText(new JsonData());
            _sl.Register(localize);
        }

        private async UniTask<UserDataDto> LoadOrCreateUserData()
        {
            var network = _sl.Get<INetworkServiceProvider>();

            var result = await network.User.GetUserData();
            if (result.IsSuccess)
                return result.data;

            var create = await network.User.CreateUserData();
            return create.data;
        }

        private async UniTask InitChart(string nickname, UserDataDto userData)
        {
            var dbService = new LocalDBLoadService();
            var db = await dbService.InitialLoad(null);

            _sl.Register<IUserRepository>(new UserRepository(nickname, userData));
            _sl.Register<IDBProvider>(db);
        }
    }
}
