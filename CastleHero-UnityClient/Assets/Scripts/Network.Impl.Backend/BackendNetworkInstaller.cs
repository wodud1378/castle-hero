using CastleHero.Common.Pattern;
using CastleHero.Network.Impl.Backend.Boot;
using CastleHero.Network.Impl.Backend.Login;
using CastleHero.Network.Impl.Backend.Services;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Boot;
using CastleHero.Network.Service.Login;
using UnityEngine;

namespace CastleHero.Network.Impl.Backend
{
    internal static class BackendNetworkInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var config = NetworkConfig.Load();
            if (config == null || config.Type != NetworkType.Backend)
                return;

            var sl = ServiceLocator.Instance;
            sl.Register<INetworkServiceProvider>(new BackendNetworkServiceProvider(sl));
            sl.Register<IBootServiceFactory>(new BackendBootServiceFactory());
            sl.Register<ILoginServiceFactory>(new BackendLoginServiceFactory());
#if UNITY_EDITOR
            sl.Register<ITestService>(new BackendTestService(sl));
#endif
        }
    }
}
