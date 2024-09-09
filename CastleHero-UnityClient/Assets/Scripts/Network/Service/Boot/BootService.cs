using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Common.InApp;
using RGLabs.Common.Localize;
using RGLabs.Common.Sound;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Network.DB.Service;
using RGLabs.Network.Shared;
using RGLabs.Network.Service.Login;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace RGLabs.Network.Service.Boot
{
    public class BootService : IDisposable
    {
        private readonly IBootServiceHandler _handler;
        private readonly IBackendErrorHandler _errorHandler;
        private readonly ILoginService _autoLoginService;
        private readonly InitService _initService;
        private readonly BootConfig _config;

        private ChartInfo[] _chartList;

        public BootService(BootConfig config, IBootServiceHandler handler, IBackendErrorHandler errorHandler = null)
        {
            _handler = handler;
            _errorHandler = errorHandler;
            _autoLoginService = new AutoLoginService();
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
                Backend.BMember.DeleteGuestInfo();
                PlayerPrefs.DeleteAll();
            }
#endif

            bool newUser = false;
            var autoLogin = await _autoLoginService.Login();
            if (!autoLogin.IsSuccess)
            {
                Debug.Log("자동 로그인 실패");

                var loginService = await _handler.ProvideLoginService();

                if (loginService is not GuestLoginService)
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
            
            // 유저 데이터 로드.
            // 신규 가입이거나 데이터 초기화 이전 종료 등으로
            // 테이블이 정상 초기화 되지 않았을 경우 데이터 생성.
            Result<UserDataDto> result;
            if (newUser)
            {
                result = await NetworkService.User.CreateUserData();
            }
            else
            {
                result = await NetworkService.User.GetUserData();

                if (!result.IsSuccess && result.statusCode == 404)
                {
                    result = await NetworkService.User.CreateUserData();
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

        private async UniTask<Result> CacheChartList()
        {
            var response = await _initService.GetChartList();
            if (!response.IsSuccess)
            {
                _handler
                    .OnError(response)
                    .Forget();

                return Result.Error(response.error);
            }

            _chartList = response.data;
            return Result.Complete();
        }

        private async UniTask<Result> InitLocalize()
        {
            var result = await _initService.GetChartContent(
                _chartList.FirstOrDefault(x => x.chartName == "localize")
                    .selectedChartFileId.ToString());

            if (!result.IsSuccess)
            {
                _handler
                    .OnError(result)
                    .Forget();

                return Result.Error(result.error);
            }

            Storage.localize = new LocalizeText(result.raw.FlattenRows());

            await Storage.localize.Set(Application.systemLanguage);

            return Result.Complete();
        }

        private async UniTask<Result> Init()
        {
            // TODO : 클라우드 프로젝트 생성 및 IAP 설정 후 주석 비활성화
            // var initUnityServices = await InitUnityServices();
            // if (!initUnityServices.IsSuccess)
            //     return Result.Error(initUnityServices.error, initUnityServices.errorMessage);

            var initServer = _initService.InitServer("dev");
            if (!initServer.IsSuccess)
            {
                _handler
                    .OnError(initServer)
                    .Forget();

                return Result.Error(initServer.error);
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

            Storage.settingRepository = new();
            Storage.soundPath = await Addressables.LoadAssetAsync<SoundPath>("Sound/SoundPath.asset");

            return Result.Complete();
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
#endif
        }

        private async UniTask<string> GetNickname()
        {
            var nickname = Backend.UserNickName;
            if (string.IsNullOrEmpty(nickname))
            {
                bool isSuccess = false;
                while (!isSuccess)
                {
                    nickname = await _handler.SetNickName();
                    var response = await NetworkService.User.UpdateNickname(nickname);

                    isSuccess = response.IsSuccess;
                }

                Debug.Log("닉네임 설정 완료");
            }

            return nickname;
        }

        private async UniTask InitChart(string nickname, UserDataDto userData)
        {
            var service = new DBLoadService(_initService);
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

            Storage.inGameRepository = new();
            Storage.userRepository = new(nickname, userData);
            Storage.db = result;

            // var inAppProducts = result.db.shop.GetInAppProducts();
            // var iap = new IAPManager(inAppProducts);
            // NetworkService.Shop.RegisterIAP(iap);
        }

        // private async UniTask<Result> InitUnityServices()
        // {
        //     try
        //     {
        //         var options = new InitializationOptions().SetEnvironmentName("production");
        //
        //         await UnityServices.InitializeAsync(options);
        //
        //         return Result.Complete();
        //     }
        //     catch (Exception e)
        //     {
        //         return Result.Error(Error.Unknown, e.ToString());
        //     }
        // }

        public void Dispose() => _errorHandler?.Detach();
    }
}