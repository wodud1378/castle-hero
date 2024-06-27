using BackEnd;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
{
    public class GuestLoginService : ILoginService
    {
        public UniTask<Response> Login() => BackendWrapper.GuestLogin();
    }
}