using System;
using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;

namespace RGLabs.Network
{
    [Serializable]
    public struct VersionInfo
    {
        public string version;
        public int type;
    }

    [Serializable]
    public struct ServerStatus
    {
        public int serverStatus;
    }

    [Serializable]
    public struct Policy
    {
        public PolicyData policy;
        public PolicyData policy2;
    }

    [Serializable]
    public struct PolicyData
    {
        public string terms;
        public string termsURL;
        public string privacy;
        public string privacyURL;
    }
    
    public static class BackendWrapper
    {
        public delegate void Api(Backend.BackendCallback onResult);

        public static async UniTask<Response> Init(string serverName)
        {
            var project = MultiSettingManager.FindByProjectName(serverName);
            var api = new Api(onResult =>
            {
                Backend.InitializeByMultiProjectAsync(project, true, true,
                    onResult.Invoke);
            }); 

            return await Call(api);
        }

        public static async UniTask<Response<VersionInfo>> CheckVersion() 
            => await Call<VersionInfo>(Backend.Utils.GetLatestVersion);

        public static async UniTask<Response<ServerStatus>> CheckServerStatus() 
            => await Call<ServerStatus>(Backend.Utils.GetServerStatus);

        public static async UniTask<Response> AutoLogin()
            => await Call(Backend.BMember.LoginWithTheBackendToken);

        public static async UniTask<Response> GuestLogin()
            => await Call(Backend.BMember.GuestLogin);
        
        public static async UniTask<Response> FederationLogin(string token, FederationType type)
        {
            var api = new Api(onResult => Backend.BMember.AuthorizeFederation(token, type, onResult.Invoke));
            return await Call(api);
        }

        public static async UniTask<Response<Policy>> GetPolicy()
            => await Call<Policy>(Backend.Policy.GetPolicyV2);

        private static async UniTask<Response> Call(Api api)
        {
            var src = new UniTaskCompletionSource<Response>();
            api.Invoke(result =>
            {
                var response = new Response(result);
                src.TrySetResult(response);
            });
            
            return await src.Task;
        }
        
        private static UniTask<Response<T>> Call<T>(Api api)
        {
            var src = new UniTaskCompletionSource<Response<T>>();
            api.Invoke(result =>
            {
                var response = new Response<T>(result);
                src.TrySetResult(response);
            });
            
            return src.Task;
        }
    }
}