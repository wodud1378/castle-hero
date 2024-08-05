using System;
using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;
using RGLabs.Utility;

namespace RGLabs.Network.Service.Boot
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
    
    public class InitService : NetworkServiceBase
    {
        public Response Init(string serverName)
        {
            var project = MultiSettingManager.FindByProjectName(serverName);
            var bro = Backend.InitializeByMultiProject(project);

            return new Response(bro);
        }
        
        public UniTask<Response<VersionInfo>> GetServerVersion()
            => Call(Backend.Utils.GetLatestVersion, raw =>
            {
                var jsonData = raw.GetReturnValuetoJSON();
                return jsonData.Cast<VersionInfo>();
            });

        public UniTask<Response<ServerStatus>> CheckServerStatus()
            => Call(Backend.Utils.GetServerStatus, raw =>
            {
                var jsonData = raw.GetReturnValuetoJSON();
                return jsonData.Cast<ServerStatus>();
            });

        public UniTask<Response<Policy>> GetPolicy()
            => Call<Policy>(Backend.Policy.GetPolicyV2);

        public UniTask<Response<ChartInfo[]>> GetChartList()
            => Call(Backend.Chart.GetChartListV2, raw =>
            {
                var jsonData = raw.FlattenRows();
                return jsonData.Cast<ChartInfo[]>();
            });

        public UniTask<Response> GetChartContent(string id)
            => Call(onResult => Backend.Chart.GetChartContents(id, onResult.Invoke));
    }
}