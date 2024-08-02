using System;
using System.Collections.Generic;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
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
        private readonly BootConfig _config;

        public BootService(BootConfig config, IBootServiceHandler handler, IBackendErrorHandler errorHandler = null)
        {
            _handler = handler;
            _autoLoginService = new AutoLoginService();
            _errorHandler = errorHandler;
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
                ? (await BackendWrapper.NewUser()).data
                : (await BackendWrapper.GetUserData()).data;

            Debug.Log("데이터 불러오기 완료");

            await InitStorage(nickname, userData);

            _handler.OnInitDone();

            Debug.Log("부팅 성공");
        }

        private async UniTask Init()
        {
            var initResult = BackendWrapper.Init("dev");
            if (initResult.result != ResultCode.Success)
                await _handler.OnError(initResult);

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
            var bro = Backend.Utils.GetLatestVersion();
            var jsonData = bro.GetReturnValuetoJSON();
            
            Debug.Log($"[GetLatestVersion] {bro.IsSuccess()}, {jsonData.ToJson()}");

            if (Application.version == jsonData["version"].ToString())
                return;

            bool foreUpdate = int.Parse(jsonData["type"].ToString()) == 2;
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
                    var response = await BackendWrapper.UpdateNickname(nickname);

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
                : new DBLoadService();

            var collections = await service.Load();

            Storage.Init(nickname, userData, collections);
        }

        public void Dispose() => _errorHandler?.Detach();
    }
}