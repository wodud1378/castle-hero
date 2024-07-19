using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.MultiSettings;
using BackendFunction;
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
        public const string INFO_TABLE = "info";
        public const string ACT_TABLE = "act";
        public const string CURRENCY_TABLE = "currency";
        public const string CHARACTERS_TABLE = "characters";
        public const string FORMATION_TABLE = "formation";
        public const string INVENTORY_TABLE = "inventory";

        private static readonly BFunc _localFunc = new();

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
        
        public static UniTask<Response<Inventory>> GetUserTable(params string[] tables)
        {
            var read = TransactionGet(INVENTORY_TABLE);
            return Call(
                onResult => Backend.GameData.TransactionReadV2(read, onResult.Invoke),
                ConvertFunctionResponse<Inventory>());
        }

        public static async UniTask<Response<UserData>> GetUserData()
        {
            var read = TransactionGet(
                INFO_TABLE,
                CURRENCY_TABLE,
                CHARACTERS_TABLE,
                FORMATION_TABLE,
                INVENTORY_TABLE
            );

            return await Call(
                onResult => Backend.GameData.TransactionReadV2(read, onResult.Invoke),
                ConvertUserData());
        }

        public static UniTask<Response> GetChartContent(string id)
            => Call(onResult => Backend.Chart.GetChartContents(id, onResult.Invoke));

        public static UniTask<Response<UserData>> NewUser()
        {
            return InvokeFunc_Local("DefaultData", null, ConvertUserData_Local());
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

            return InvokeFunc(method, parameters, ConvertFunctionResponse<GrowthResult>());
        }

        public static UniTask<Response<Inventory>> TEST_AddItem(int[] itemIds, int[] quantities)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(itemIds), itemIds),
                new(nameof(quantities), quantities),
            };

            return InvokeFunc("AddItems", parameters, ConvertFunctionResponse<Inventory>());
        }

        public static UniTask<Response<Currency>> TEST_AddCurrency(int paidDia, int freeDia, int gold)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(paidDia), paidDia),
                new(nameof(freeDia), freeDia),
                new(nameof(gold), gold)
            };

            return InvokeFunc("AddCurrency", parameters, ConvertFunctionResponse<Currency>());
        }

        private static UniTask<Response> InvokeFunc(string functionName,
            List<KeyValuePair<string, object>> parameters)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }

        private static UniTask<Response<T>> InvokeFunc<T>(string functionName,
            List<KeyValuePair<string, object>> parameters, Response<T>.ConvertFromBackend convert)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke), convert);
        }
        
        private static Response<T>.ConvertFromBackend ConvertFunctionResponse<T>() => raw => NetworkHelper.Cast<T>(raw.GetFlattenJSON()["data"]);

        private static Response<UserData>.ConvertFromBackend ConvertUserData() =>
            raw =>
            {
                var json = raw.GetFlattenJSON()["data"];
                return new UserData
                {
                    info = NetworkHelper.Cast<Info>(json[INFO_TABLE]),
                    act = NetworkHelper.Cast<Act>(json[ACT_TABLE]),
                    currency = NetworkHelper.Cast<Currency>(json[CURRENCY_TABLE]),
                    characters = NetworkHelper.Cast<Characters>(json[CHARACTERS_TABLE]),
                    formation = NetworkHelper.Cast<Formation>(json[FORMATION_TABLE]),
                    inventory = NetworkHelper.Cast<Inventory>(json[INVENTORY_TABLE]),
                };
            };

        private static UniTask<Response<T>> InvokeFunc_Local<T>(string functionName, List<KeyValuePair<string, object>> parameters,
            Response<T>.ConvertFromLocal convert)
        {
            var param = FunctionParam_Local(functionName, parameters);
            var json = _localFunc.Invoke(param);

            return UniTask.FromResult(new Response<T>(json, convert));
        }

        private static Response<T>.ConvertFromLocal ConvertUserTable_Local<T>() => NetworkHelper.Cast<T>;
        
        private static Response<UserData>.ConvertFromLocal ConvertUserData_Local() =>
            json => new UserData
            {
                info = NetworkHelper.Cast<Info>(json[INFO_TABLE]),
                act = NetworkHelper.Cast<Act>(json[ACT_TABLE]),
                currency = NetworkHelper.Cast<Currency>(json[CURRENCY_TABLE]),
                characters = NetworkHelper.Cast<Characters>(json[CHARACTERS_TABLE]),
                formation = NetworkHelper.Cast<Formation>(json[FORMATION_TABLE]),
                inventory = NetworkHelper.Cast<Inventory>(json[INVENTORY_TABLE]),
            };

        private static Param FunctionParam(string functionName, List<KeyValuePair<string, object>> parameters = null)
        {
            var param = new Param { { "functionName", functionName } };
            if (parameters == null) 
                return param;
            
            foreach (var kvp in parameters)
            {
                param.Add(kvp.Key, kvp.Value);
            }
            
            return param;
        }

        private static JsonData FunctionParam_Local(string functionName,
            List<KeyValuePair<string, object>> parameters = null)
        {
            var param = new JsonData
            {
                ["functionName"] = functionName
            };

            if (parameters == null) 
                return param;
            
            foreach (var kvp in parameters)
            {
                param[kvp.Key] = kvp.Value.ToString();
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

        private static UniTask<Response<T>> Call<T>(Api api, Response<T>.ConvertFromBackend convertFromBackend = null)
        {
            var src = new UniTaskCompletionSource<Response<T>>();
            api.Invoke(result =>
            {
                var response = new Response<T>(result, convertFromBackend);
                src.TrySetResult(response);
            });

            return src.Task;
        }
    }
}