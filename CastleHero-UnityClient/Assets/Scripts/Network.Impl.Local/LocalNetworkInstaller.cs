using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using UnityEngine;

namespace CastleHero.Network.Impl.Local
{
    /// <summary>
    /// CASTLEHERO_LOCAL_NETWORK 스크립팅 디파인이 활성화된 경우에만 동작.
    /// LocalNetworkServiceProvider 를 ServiceLocator 에 등록한다.
    /// </summary>
    internal static class LocalNetworkInstaller
    {
#if CASTLEHERO_LOCAL_NETWORK
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            ServiceLocator.Register<INetworkServiceProvider>(new LocalNetworkServiceProvider());
        }
#endif
    }
}
