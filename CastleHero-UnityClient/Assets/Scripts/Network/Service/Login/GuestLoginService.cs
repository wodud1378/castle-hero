using BackEnd;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
{
    public class GuestLoginService : NetworkServiceBase, ILoginService
    {
        public async UniTask<Response> Login()
        {
            Response response = null;
            for (int i = 0; i < 3 && response is not { IsSuccess: true }; ++i)
            {
                response = await Call(Backend.BMember.GuestLogin);
            }

            return response;
        }
    }
}