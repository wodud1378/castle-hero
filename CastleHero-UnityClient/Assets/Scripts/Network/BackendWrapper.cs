using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Utility;

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

    public static partial class BackendWrapper
    {
        private const string InfoTable = "info";
        private const string CurrencyTable = "currency";
        private const string CharactersTable = "characters";
        private const string FormationTable = "formation";
        private const string InventoryTable = "inventory";

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

        public static UniTask<Response<SummonResult>> Summon(int count, int eventIndex, int eventChartId,
            int listChartId)
        {
            var param = new Param
            {
                { "functionName", $"SummonX{count}" },
                { "eventIndex", eventIndex },
                { "eventChartId", eventChartId },
                { "listChartId", listChartId },
            };

            return Call<SummonResult>(
                onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }

        public static async UniTask<Response<UserData>> GetUserData()
        {
            var read = TransactionGet(
                InfoTable,
                CurrencyTable,
                CharactersTable,
                FormationTable,
                InventoryTable
            );

            return await Call(
                onResult => Backend.GameData.TransactionReadV2(read, onResult.Invoke),
                (raw) =>
                {
                    var json = raw.GetFlattenJSON();
                    return new UserData
                    {
                        info = json[InfoTable].Cast<Info>(),
                        currency = json[CurrencyTable].Cast<Currency>(),
                        characters = json[CharactersTable].Cast<Characters>(),
                        formation = json[FormationTable].Cast<Formation>(),
                        inventory = json[InfoTable].Cast<Inventory>(),
                    };
                });
        }

        public static UniTask<Response> GetChartContent(string id)
            => Call(onResult => Backend.Chart.GetChartContents(id, onResult.Invoke));

        public static async UniTask<Response<UserData>> NewUser()
        {
            var response = await InvokeFunc("DefaultData");

            return await GetUserData();
        }

        public static UniTask<Response<GrowthResult>> Growth(string method, int chartId, int unitId, int itemId,
            int itemQty)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new("chartId", chartId),
                new("itemChartId", Storage.db.items.Id),
                new(nameof(unitId), unitId),
                new(nameof(itemId), itemId),
                new(nameof(itemQty), itemQty),
            };

            return InvokeFunc<GrowthResult>(method, parameters);
        }

        public static UniTask<Response<Inventory>> TEST_AddItem(int[] itemIds, int[] quantities)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(itemIds), itemIds),
                new(nameof(quantities), quantities),
            };

            return InvokeFunc<Inventory>("AddItems", parameters);
        }

        public static UniTask<Response<Currency>> TEST_AddCurrency(int paidDia, int freeDia, int gold)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(paidDia), paidDia),
                new(nameof(freeDia), freeDia),
                new(nameof(gold), gold)
            };

            return InvokeFunc<Currency>("AddCurrency", parameters);
        }

        private static UniTask<Response> InvokeFunc(string functionName,
            List<KeyValuePair<string, object>> parameters = null)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }

        private static UniTask<Response<T>> InvokeFunc<T>(string functionName,
            List<KeyValuePair<string, object>> parameters = null)
        {
            var param = FunctionParam(functionName, parameters);
            return Call<T>(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }

        private static Param FunctionParam(string functionName, List<KeyValuePair<string, object>> parameters = null)
        {
            var param = new Param { { "functionName", functionName } };
            if (parameters != null)
            {
                foreach (var kvp in parameters)
                {
                    param.Add(kvp.Key, kvp.Value);
                }
            }

            return param;
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

        private static UniTask<Response<T>> Call<T>(Api api, Response<T>.Convert convert = null)
        {
            var src = new UniTaskCompletionSource<Response<T>>();
            api.Invoke(result =>
            {
                var response = new Response<T>(result, convert);
                src.TrySetResult(response);
            });

            return src.Task;
        }
    }
}