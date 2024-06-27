using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
{
    public class AutoLoginService : ILoginService
    {
        public UniTask<Response> Login() => BackendWrapper.AutoLogin();
    }
}