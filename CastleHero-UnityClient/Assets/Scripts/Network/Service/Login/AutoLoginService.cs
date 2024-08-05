using BackEnd;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
{
    public class AutoLoginService : NetworkServiceBase, ILoginService
    {
        public UniTask<Response> Login() => Call(Backend.BMember.LoginWithTheBackendToken);
    }
}