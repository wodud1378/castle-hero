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
                var loginService = await _handler.ProvideLoginService();
                var loginResponse = await loginService.Login();
                var code = loginResponse.statusCode;
                newUser = code == 201;
                if (newUser)
                    await _handler.CheckPolicy();
            }

            var nickname = await GetNickname();
            var userData = newUser
                ? (await BackendWrapper.NewUser()).data
                : (await BackendWrapper.GetUserData()).data;

            await InitStorage(nickname, userData);

            _handler.OnInitDone();
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
            var version = await BackendWrapper.CheckVersion();
            if(version.result != ResultCode.Success)
                await _handler.OnError(version);

            if (version.data.type == 2)
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