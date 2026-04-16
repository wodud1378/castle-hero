using Cysharp.Threading.Tasks;
using CastleHero.Network.Service.Login;

namespace CastleHero.Network.Service.Boot
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