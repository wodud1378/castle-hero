using BackEnd;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Pattern;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Login;
using GooglePlayGames;
using GooglePlayGames.BasicApi;

namespace CastleHero.Network.Impl.Backend.Login
{
    public class BackendGPGSLoginService : BackendNetworkServiceBase, ILoginService
    {
        public BackendGPGSLoginService(IServiceLocator sl) : base(sl) { }

        public async UniTask<Result> Login()
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
                global::BackEnd.Backend.BMember.AuthorizeFederation(result.token, FederationType.Google, onResult.Invoke));

            return await Call(api);
        }

        private UniTask<(bool success, string token)> GetToken()
        {
            var src = new UniTaskCompletionSource<(bool success, string token)>();
            var gpgs = PlayGamesPlatform.Instance;

            if (gpgs.IsAuthenticated())
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
