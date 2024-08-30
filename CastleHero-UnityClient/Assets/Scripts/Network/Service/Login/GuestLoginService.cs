using BackEnd;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network.Service.Login
{
    public class GuestLoginService : NetworkServiceBase, ILoginService
    {
        public async UniTask<Result> Login()
        {
            Result result = null;
            for (int i = 0; i < 3 && result is not { IsSuccess: true }; ++i)
            {
                result = await Call(Backend.BMember.GuestLogin);
            }

            return result;
        }
    }
}