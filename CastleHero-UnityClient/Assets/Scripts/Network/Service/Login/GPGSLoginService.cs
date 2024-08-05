using BackEnd;
using Cysharp.Threading.Tasks;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace RGLabs.Network.Service.Login
{
    public class GPGSLoginService : NetworkServiceBase, ILoginService
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
            var api = new Api(onResult =>
                Backend.BMember.AuthorizeFederation(result.token, FederationType.Google, onResult.Invoke));
            
            return await Call(api);
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