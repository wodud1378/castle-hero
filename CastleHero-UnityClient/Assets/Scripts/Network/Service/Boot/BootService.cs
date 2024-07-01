using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using RGLabs.Data;
using RGLabs.Network.DB;
using RGLabs.Network.Service.Login;
using UnityEngine.AddressableAssets;

namespace RGLabs.Network.Service.Boot
{
    public class BootService : IDisposable
    {
        private readonly IBootServiceHandler _handler;
        private readonly IBackendErrorHandler _errorHandler;
        private readonly ILoginService _autoLoginService;
        private readonly BootConfig _config;
        
        private readonly Chart _chart = new();

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
            
            var isSuccess = await AutoLogin();
            if (!isSuccess)
                await _handler.OnNeedLogin();

            await InitStorage();
            
            _handler.OnInitDone();
        }

        private async UniTask Init()
        {
            var initResult = await BackendWrapper.Init("Dev");
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

        private async UniTask<bool> AutoLogin()
        {
            var response = await _autoLoginService.Login();
            return response.result == ResultCode.Success;
        }

        private async UniTask InitStorage()
        {
            DBCollections collections;
            if (_config.useLocalDatabase)
                collections = await _chart.LoadFromLocal();
            else
                collections = await _chart.LoadFromServer();

            Storage.Init(collections);
        }

        public void Dispose() => _errorHandler?.Detach();
    }
}