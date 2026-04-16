using System;
using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;
using CastleHero.Network.Service;
using CastleHero.Network.Service.Boot;
using CastleHero.Utility;

namespace CastleHero.Network.Impl.Backend.Boot
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

    public class BackendInitService : BackendNetworkServiceBase
    {
        public BackendResult InitServer(string serverName)
        {
            var project = MultiSettingManager.FindByProjectName(serverName);
            var raw = global::BackEnd.Backend.InitializeByMultiProject(project);

            return BackendResult.Complete(raw);
        }

        public UniTask<BackendResult<VersionInfo>> GetServerVersion()
            => Call(global::BackEnd.Backend.Utils.GetLatestVersion, raw =>
            {
                var jsonData = raw.GetReturnValuetoJSON();
                return jsonData.Cast<VersionInfo>();
            });

        public UniTask<BackendResult<ServerStatus>> CheckServerStatus()
            => Call(global::BackEnd.Backend.Utils.GetServerStatus, raw =>
            {
                var jsonData = raw.GetReturnValuetoJSON();
                return jsonData.Cast<ServerStatus>();
            });

        public UniTask<BackendResult<Policy>> GetPolicy()
            => Call<Policy>(global::BackEnd.Backend.Policy.GetPolicyV2);

        public UniTask<BackendResult<ChartInfo[]>> GetChartList()
            => Call(global::BackEnd.Backend.Chart.GetChartListV2, raw =>
            {
                var jsonData = raw.FlattenRows();
                return jsonData.Cast<ChartInfo[]>();
            });

        public UniTask<BackendResult> GetChartContent(string id)
            => Call(onResult => global::BackEnd.Backend.Chart.GetChartContents(id, onResult.Invoke));
    }
}
