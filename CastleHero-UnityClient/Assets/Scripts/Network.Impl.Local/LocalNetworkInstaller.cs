using CastleHero.Common.Pattern;
using CastleHero.Network.Impl.Local.Services;
using CastleHero.Network.Service;
using UnityEngine;

namespace CastleHero.Network.Impl.Local
{
    /// <summary>
    /// CASTLEHERO_LOCAL_NETWORK 스크립팅 디파인이 활성화된 경우에만 동작.
    /// LocalUserDataStore 를 만들어 LocalNetworkServiceProvider 에 주입하고 ServiceLocator 에 등록한다.
    /// </summary>
    internal static class LocalNetworkInstaller
    {
#if CASTLEHERO_LOCAL_NETWORK
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            var store = new LocalUserDataStore();
            ServiceLocator.Register(store);
            ServiceLocator.Register<INetworkServiceProvider>(new LocalNetworkServiceProvider(store));
#if UNITY_EDITOR
            ServiceLocator.Register<ITestService>(new LocalTestService(store));
#endif
        }
#endif
    }
}
