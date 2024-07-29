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
            var config = new PlayGamesClientConfiguration.Builder()
                .RequestServerAuthCode(false)
                .RequestEmail()
                .RequestIdToken()
                .Build();
            
            PlayGamesPlatform.InitializeInstance(config);
            PlayGamesPlatform.DebugLogEnabled = true;

            PlayGamesPlatform.Activate();
            
            var result = await GetToken();
            return await BackendWrapper.FederationLogin(result.token, FederationType.Google);
        }

        private UniTask<(bool success, string token)> GetToken()
        {
            var src = new UniTaskCompletionSource<(bool success, string token)>();
            var gpgs = PlayGamesPlatform.Instance;
            
            if(gpgs.IsAuthenticated())
                gpgs.SignOut();
            
            gpgs.Authenticate(SignInInteractivity.CanPromptAlways, result =>
            {
                src.TrySetResult(
                    result == SignInStatus.Success 
                        ? (true, gpgs.GetIdToken()) 
                        : (false, string.Empty));
            });

            return src.Task;
        }
    }
}