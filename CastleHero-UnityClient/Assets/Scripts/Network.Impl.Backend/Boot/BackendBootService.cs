using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Localize;
using CastleHero.Common.Pattern;
using CastleHero.Common.Sound;
using CastleHero.Data;
using CastleHero.Data.DB;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using CastleHero.Network.Impl.Backend.DB;
using CastleHero.Network.Impl.Backend.Login;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Boot;
using CastleHero.Network.Service.Login;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace CastleHero.Network.Impl.Backend.Boot
{
    public class BackendBootService : IBootService, IDisposable
    {
        private readonly IBootServiceHandler _handler;
        private readonly IBackendErrorHandler _errorHandler;
        private readonly ILoginService _autoLoginService;
        private readonly BackendInitService _initService;
        private readonly BootConfig _config;

        private ChartInfo[] _chartList;

        public BackendBootService(BootConfig config, IBootServiceHandler handler, IBackendErrorHandler errorHandler = null)
        {
            _handler = handler;
            _errorHandler = errorHandler;
            _autoLoginService = new BackendAutoLoginService();
            _initService = new();
            _config = config;

            _errorHandler?.Attach();
        }

        public async UniTask Start()
        {
            Debug.Log("부팅 시작");

            await Init();
            await CheckVersion();

#if UNITY_EDITOR
            if (_config.deleteGuestId)
            {
                global::BackEnd.Backend.BMember.DeleteGuestInfo();
                PlayerPrefs.DeleteAll();
            }
#endif

            bool newUser = false;
            var autoLogin = await _autoLoginService.Login();
            if (!autoLogin.IsSuccess)
            {
                Debug.Log("자동 로그인 실패");

                var loginService = await _handler.ProvideLoginService();

                if (loginService is not BackendGuestLoginService)
                    Debug.Log("페더레이션 로그인 진행");

                var loginResponse = await loginService.Login();
                var code = loginResponse.statusCode;
                newUser = code == 201;
            }

            Debug.Log("로그인 성공");

            await CacheChartList();
            await InitLocalize();
            await _handler.CheckPolicy();

            var nickname = await GetNickname();

            Debug.Log(newUser
                ? "신규 유저 로그인"
                : "기존 유저 로그인");

            Result<UserDataDto> result;
            if (newUser)
            {
                result = await ServiceLocator.Get<INetworkServiceProvider>().User.CreateUserData();
            }
            else
            {
                result = await ServiceLocator.Get<INetworkServiceProvider>().User.GetUserData();

                if (!result.IsSuccess && result.statusCode == 404)
                {
                    result = await ServiceLocator.Get<INetworkServiceProvider>().User.CreateUserData();
                }
            }

            if (!result.IsSuccess)
            {
                await _handler.OnError(result);
            }

            Debug.Log("데이터 불러오기 완료");

            await InitChart(nickname, result.data);

            _handler.OnInitDone();

            Debug.Log("부팅 성공");
        }

        private async UniTask<BackendResult> CacheChartList()
        {
            var response = await _initService.GetChartList();
            if (!response.IsSuccess)
            {
                _handler
                    .OnError(response)
                    .Forget();

                return BackendResult.Error(response.error);
            }

            _chartList = response.data;
            return BackendResult.Complete(response.raw);
        }

        private async UniTask<BackendResult> InitLocalize()
        {
            var result = await _initService.GetChartContent(
                _chartList.FirstOrDefault(x => x.chartName == "localize")
                    .selectedChartFileId.ToString());

            if (!result.IsSuccess)
            {
                _handler
                    .OnError(result)
                    .Forget();

                return BackendResult.Error(result.error);
            }

            var localize = new LocalizeText(result.raw.FlattenRows());
            ServiceLocator.Register(localize);

            await localize.Set(Application.systemLanguage);

            return BackendResult.Complete(result.raw);
        }

        private async UniTask<BackendResult> Init()
        {
            var initServer = _initService.InitServer("dev");
            if (!initServer.IsSuccess)
            {
                _handler
                    .OnError(initServer)
                    .Forget();

                return BackendResult.Error(initServer.error);
            }

            await Addressables.InitializeAsync();
            var catalogs = await Addressables.CheckForCatalogUpdates();
            var tasks = new List<UniTask>();
            foreach (var catalog in catalogs)
            {
                var handle = Addressables.DownloadDependenciesAsync(catalog);
                tasks.Add(handle.ToUniTask());
            }

            await UniTask.WhenAll(tasks);

            // EntranceHolder 는 GameHandler / UIStageSelect / UILobby 등이 이른 시점부터 참조하므로 최우선 등록.
            ServiceLocator.Register(new EntranceHolder());

            ServiceLocator.Register(new SettingRepository());
            ServiceLocator.Register(await Addressables.LoadAssetAsync<SoundPath>("Sound/SoundPath.asset"));

            return BackendResult.Complete(initServer.raw);
        }

        private async UniTask CheckVersion()
        {
#if !UNITY_EDITOR
            var response = await _initService.GetServerVersion();
            if (!response.IsSuccess)
            {
                _handler
                    .OnError(response)
                    .Forget();

                return;
            }

            var versionInfo = response.data;
            Debug.Log($"[GetLatestVersion] {versionInfo.ToJson()}");

            if (Application.version == versionInfo.version)
                return;

            bool foreUpdate = versionInfo.type == 2;
            if(foreUpdate)
                await _handler.OnForceUpdate();
#else
            await UniTask.DelayFrame(1);
#endif
        }

        private async UniTask<string> GetNickname()
        {
            var nickname = global::BackEnd.Backend.UserNickName;
            if (string.IsNullOrEmpty(nickname))
            {
                bool isSuccess = false;
                while (!isSuccess)
                {
                    nickname = await _handler.SetNickName();
                    var response = await ServiceLocator.Get<INetworkServiceProvider>().User.UpdateNickname(nickname);

                    isSuccess = response.IsSuccess;
                }

                Debug.Log("닉네임 설정 완료");
            }

            return nickname;
        }

        private async UniTask InitChart(string nickname, UserDataDto userData)
        {
            var service = new BackendDBLoadService(_initService);
            var response = await _initService.GetChartList();
            if (!response.IsSuccess)
            {
                _handler
                    .OnError(response)
                    .Forget();

                return;
            }

            var chartList = response.data;
            var result = await service.InitialLoad(chartList);

            ServiceLocator.Register<IUserRepository>(new UserRepository(nickname, userData));
            ServiceLocator.Register<IDBProvider>(result);
        }

        public void Dispose() => _errorHandler?.Detach();
    }
}
