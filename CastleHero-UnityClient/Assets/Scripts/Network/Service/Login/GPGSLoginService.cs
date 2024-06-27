using BackEnd;
using Cysharp.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace RGLabs.Network.Service.Login
{
    public class GPGSLoginService : ILoginService
    {
        public async UniTask<Response> Login()
        {
            var result = await GetToken();
            return await BackendWrapper.FederationLogin(result.token, FederationType.Google);
        }

        private UniTask<(bool success, string token)> GetToken()
        {
            var src = new UniTaskCompletionSource<(bool success, string token)>();
            var gpgs = PlayGamesPlatform.Instance;
            gpgs.Authenticate(res =>
            {
                if (res == SignInStatus.Success)
                {
                    gpgs.RequestServerSideAccess(false,
                        token => src.TrySetResult((true, token)));
                }
                else
                {
                    src.TrySetResult((false, string.Empty));
                }
            });

            return src.Task;
        }
    }
}