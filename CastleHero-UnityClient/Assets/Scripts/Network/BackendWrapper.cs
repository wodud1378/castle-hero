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

    [Serializable]
    public struct ChartInfo
    {
        public string chartName;
        public string chartExplain;
        public int selectedChartFileId;
    }
    
    public static class BackendWrapper
    {
        public delegate void Api(Backend.BackendCallback onResult);

        public static UniTask<Response> Init(string serverName)
        {
            var project = MultiSettingManager.FindByProjectName(serverName);
            var api = new Api(onResult =>
            {
                Backend.InitializeByMultiProjectAsync(project, true, true,
                    onResult.Invoke);
            });

            return Call(api);
        }

        public static UniTask<Response<VersionInfo>> CheckVersion() 
            => Call<VersionInfo>(Backend.Utils.GetLatestVersion);

        public static UniTask<Response<ServerStatus>> CheckServerStatus() 
            => Call<ServerStatus>(Backend.Utils.GetServerStatus);

        public static UniTask<Response> AutoLogin()
            => Call(Backend.BMember.LoginWithTheBackendToken);

        public static UniTask<Response> GuestLogin()
            => Call(Backend.BMember.GuestLogin);
        
        public static UniTask<Response> FederationLogin(string token, FederationType type)
        {
            var api = new Api(onResult => Backend.BMember.AuthorizeFederation(token, type, onResult.Invoke));
            return Call(api);
        }

        public static UniTask<Response<Policy>> GetPolicy()
            => Call<Policy>(Backend.Policy.GetPolicyV2);

        public static UniTask<Response<ChartInfo[]>> GetChartList() 
            => Call<ChartInfo[]>(Backend.Chart.GetChartListV2);

        public static async UniTask<(string chartName, Response response)> GetChartContent(string chartName, string id)
        {
            var response = await Call(onResult => Backend.Chart.GetChartContents(id, onResult.Invoke));

            return (chartName, response);
        }

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