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
    /// <summary>
    /// 앱 시작 전에 Backend 구현체들을 ServiceLocator 에 등록한다.
    /// CASTLEHERO_LOCAL_NETWORK 스크립팅 디파인이 활성화된 경우 Impl.Local 이 대신 등록된다.
    /// 등록 대상: INetworkServiceProvider, IBootServiceFactory, ILoginServiceFactory.
    /// </summary>
    internal static class BackendNetworkInstaller
    {
#if !CASTLEHERO_LOCAL_NETWORK
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Install()
        {
            ServiceLocator.Register<INetworkServiceProvider>(new BackendNetworkServiceProvider());
            ServiceLocator.Register<IBootServiceFactory>(new BackendBootServiceFactory());
            ServiceLocator.Register<ILoginServiceFactory>(new BackendLoginServiceFactory());
#if UNITY_EDITOR
            ServiceLocator.Register<ITestService>(new BackendTestService());
#endif
        }
#endif
    }
}
