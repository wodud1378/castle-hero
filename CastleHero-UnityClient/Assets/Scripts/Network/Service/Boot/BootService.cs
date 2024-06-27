using System;
using BackEnd;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Service.Login;

namespace RGLabs.Network.Service.Boot
{
    public class BootService : IDisposable
    {
        private readonly IBootServiceHandler _handler;
        private readonly IBackendErrorHandler _errorHandler;
        private readonly ILoginService _autoLoginService;

        public BootService(IBootServiceHandler handler, IBackendErrorHandler errorHandler = null)
        {
            _handler = handler;
            _autoLoginService = new AutoLoginService();
            _errorHandler = errorHandler;
            _errorHandler?.Attach();
        }
        
        public async UniTask Start()
        {
            await Init();
            await CheckVersion();
            
            var isSuccess = await AutoLogin();
            if (!isSuccess)
                await _handler.OnNeedLogin();
        }

        private async UniTask Init()
        {
            var initResult = await BackendWrapper.Init("Dev");
            if (initResult.result != ResultCode.Success)
                await _handler.OnError(initResult);
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

        public void Dispose() => _errorHandler?.Detach();
    }
}