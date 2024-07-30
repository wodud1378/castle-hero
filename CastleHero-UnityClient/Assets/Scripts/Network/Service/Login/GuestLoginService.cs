using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
{
    public class GuestLoginService : ILoginService
    {
        public async UniTask<Response> Login()
        {
            Response response = null;
            for (int i = 0; i < 3 && (response == null || response.result == ResultCode.Success); ++i)
            {
                response = await BackendWrapper.GuestLogin();
            }

            return response;
        }
    }
}