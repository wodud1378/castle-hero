using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Boot
{
    public interface IBootServiceHandler
    {
        public UniTask OnError(Response response);

        public UniTask OnMaintenance();
        public UniTask OnForceUpdate();

        public UniTask OnNeedLogin();
    }
}