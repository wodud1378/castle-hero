using CastleHero.Common.Pattern;
using CastleHero.Network.Impl.Local.Boot;
using CastleHero.Network.Impl.Local.Services;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Boot;
using CastleHero.Network.Service.Login;
using UnityEngine;

namespace CastleHero.Network.Impl.Local
{
    internal static class LocalNetworkInstaller
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var config = NetworkConfig.Load();
            if (config == null || config.Type != NetworkType.Local)
                return;

            var sl = ServiceLocator.Instance;
            var store = new LocalUserDataStore();
            sl.Register(store);
            sl.Register<INetworkServiceProvider>(new LocalNetworkServiceProvider(sl, store));
            sl.Register<IBootServiceFactory>(new LocalBootServiceFactory());
            sl.Register<ILoginServiceFactory>(new LocalLoginServiceFactory());
#if UNITY_EDITOR
            sl.Register<ITestService>(new LocalTestService(sl, store));
#endif
        }
    }
}
