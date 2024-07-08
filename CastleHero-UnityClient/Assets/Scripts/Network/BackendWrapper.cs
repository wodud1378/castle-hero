using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Network.Model;

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
            => Call(onResult => Backend.BMember.AuthorizeFederation(token, type, onResult.Invoke));

        public static UniTask<Response<Policy>> GetPolicy()
            => Call<Policy>(Backend.Policy.GetPolicyV2);

        public static UniTask<Response<ChartInfo[]>> GetChartList()
            => Call<ChartInfo[]>(Backend.Chart.GetChartListV2);

        public static UniTask<Response<List<ISummonResult>>> Summon(int count, int eventIndex, int eventChartId, int listChartId)
        {
            var param = new Param
            {
                { "functionName", $"SummonX{count}" },
                { "eventIndex", eventIndex },
                { "eventChartId", eventChartId },
                { "listChartId", listChartId },
            };

            return Call<List<ISummonResult>>(
                onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }
        
        public static UniTask<Response<UserInfo>> GetUserInfo() 
            => Call<UserInfo>(onResult => Backend.GameData.GetMyData("userdata", new Where(), onResult.Invoke));

        public static UniTask<Response> GetChartContent(string id) 
            => Call(onResult => Backend.Chart.GetChartContents(id, onResult.Invoke));

        public static async UniTask<Response<UserInfo>> NewUser()
        {
            var api = NewDataFromServer();

            await Call(api);

            return await GetUserInfo();
        }

        private static Api NewDataFromServer()
        {
            var param = new Param { { "functionName", "DefaultData" } };
            return onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke);
        }

        private static Api NewDataFromLocal()
        {
            var userInfo = new UserInfo
            {
                stage = 1,
                focusedStage = 1,
                castleLv = 1,
                characters = new List<UnitInfo>()
                {
                    new()
                    {
                        id = 10001,
                        lv = 1,
                    }
                },
                fieldCharacters = new(),
                items = new()
            };

            var param = new Param
            {
                { nameof(userInfo.stage), userInfo.stage },
                { nameof(userInfo.focusedStage), userInfo.focusedStage },
                { nameof(userInfo.castleLv), userInfo.castleLv },
                { nameof(userInfo.gold), userInfo.gold },
                { nameof(userInfo.freeDia), userInfo.freeDia },
                { nameof(userInfo.paidDia), userInfo.paidDia },
                { nameof(userInfo.characters), userInfo.characters },
                { nameof(userInfo.fieldCharacters), userInfo.fieldCharacters },
                { nameof(userInfo.items), userInfo.items }
            };

            var api = new Api(
                onResult => Backend.GameData.Insert("userdata", param, onResult));

            return api;
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