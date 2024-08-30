using Cysharp.Threading.Tasks;
using RGLabs.Network.Service.Login;
using RGLabs.Title;

namespace RGLabs.Network.Service.Boot
{
    public interface IBootServiceHandler
    {
        public UniTask OnError(Result response);

        public UniTask OnMaintenance();
        public UniTask OnForceUpdate();

        public UniTask<ILoginService> ProvideLoginService();

        public UniTask<PolicyAgreement> CheckPolicy();

        public UniTask<string> SetNickName();

        public void OnInitDone();
    }
}