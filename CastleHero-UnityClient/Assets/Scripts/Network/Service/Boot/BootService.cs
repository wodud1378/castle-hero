using System;
using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.DB.Service;
using RGLabs.Network.Shared;
using RGLabs.Network.Service.Login;
using RGLabs.Network.Service.User;
using RGLabs.Utility;
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
        private readonly UserService _userService;
        private readonly BootConfig _config;

        public BootService(BootConfig config, IBootServiceHandler handler, IBackendErrorHandler errorHandler = null)
        {
            _handler = handler;
            _errorHandler = errorHandler;
            _autoLoginService = new AutoLoginService();
            _initService = new();
            _userService = new();
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
                if (newUser)
                    await _handler.CheckPolicy();
            }

            Debug.Log("로그인 성공");

            var nickname = await GetNickname();

            Debug.Log(newUser
                ? "신규 유저 로그인"
                : "기존 유저 로그인");

            var userData = newUser
                ? (await _userService.NewUser()).data
                : (await _userService.GetUserData()).data;

            Debug.Log("데이터 불러오기 완료");

            await InitStorage(nickname, userData);

            _handler.OnInitDone();

            Debug.Log("부팅 성공");
        }

        private async UniTask Init()
        {
            var initResult = _initService.Init("dev");
            if (!initResult.IsSuccess)
            {
                _handler
                    .OnError(initResult)
                    .Forget();
                
                return;
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
                    var response = await _userService.UpdateNickname(nickname);

                    isSuccess = response.IsSuccess;
                }

                Debug.Log("닉네임 설정 완료");
            }

            return nickname;
        }

        private async UniTask InitStorage(string nickname, UserData userData)
        {
            IDBLoadService service = _config.useLocalDatabase
                ? new LocalDBLoadService()
                : new DBLoadService(_initService);

            var collections = await service.Load();

            Storage.Init(nickname, userData, collections);
        }

        public void Dispose() => _errorHandler?.Detach();
    }
}