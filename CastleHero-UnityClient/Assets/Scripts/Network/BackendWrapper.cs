using System;
using System.Collections.Generic;
using BackEnd;
using BackEnd.MultiSettings;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;

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
        public const string PROFILE_TABLE = "profile";
        public const string ACT_TABLE = "act";
        public const string CURRENCY_TABLE = "currency";
        public const string CHARACTERS_TABLE = "characters";
        public const string FORMATION_TABLE = "formation";
        public const string INVENTORY_TABLE = "inventory";

        public static int LeftRequestCount { get; private set; }

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

        public static UniTask SaveFormation(Formation formation) => Save(FORMATION_TABLE, formation);

        private static UniTask Save(string tableName, object obj)
        {
            var param = new Param { { tableName, obj.ToJson() } };
            var api = new Api(onResult =>
            {
                Backend.GameData.Update(tableName, new Where(), param, onResult.Invoke);
            });

            return Call(api);
        }

        public static async UniTask<Response<UserData>> GetUserData()
        {
            var read = TransactionGet(
                PROFILE_TABLE,
                ACT_TABLE,
                CURRENCY_TABLE,
                CHARACTERS_TABLE,
                FORMATION_TABLE,
                INVENTORY_TABLE
            );

            return await Call(
                onResult => Backend.GameData.TransactionReadV2(read, onResult.Invoke),
                raw =>
                {
                    var data = raw.GetFlattenJSON();
                    var userData = new UserData
                    {
                        profile = FromTransaction<Profile>(data, PROFILE_TABLE),
                        act = FromTransaction<Act>(data, ACT_TABLE),
                        currency = FromTransaction<Currency>(data, CURRENCY_TABLE),
                        characters = FromTransaction<Characters>(data, CHARACTERS_TABLE),
                        formation = FromTransaction<Formation>(data, FORMATION_TABLE),
                        inventory = FromTransaction<Inventory>(data, INVENTORY_TABLE)
                    };

                    return userData;
                });
        }

        public static UniTask<Response> GetChartContent(string id)
            => Call(onResult => Backend.Chart.GetChartContents(id, onResult.Invoke));

        public static UniTask<Response<UserData>> NewUser()
        {
            return InvokeFunc("DefaultData", null, raw =>
            {
                var json = JsonMapper.ToObject(raw.GetFlattenJSON()["result"].ToString());
                return new UserData
                {
                    profile = json[PROFILE_TABLE].Cast<Profile>(),
                    act = json[ACT_TABLE].Cast<Act>(),
                    currency = json[CURRENCY_TABLE].Cast<Currency>(),
                    characters = json[CHARACTERS_TABLE].Cast<Characters>(),
                    formation = json[FORMATION_TABLE].Cast<Formation>(),
                    inventory = json[INVENTORY_TABLE].Cast<Inventory>(),
                };
            });
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

        public static UniTask<Response<OpenBoxResult>> OpenBox(int itemChartId, int statChartId, int boxItemId,
            int itemQty)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(itemChartId), itemChartId),
                new(nameof(statChartId), statChartId),
                new(nameof(boxItemId), boxItemId),
                new(nameof(itemQty), itemQty),
            };

            return InvokeFunc("OpenBox", parameters, ConvertFunctionResponse<OpenBoxResult>());
        }

        public static UniTask<Response<SummonResult>> Summon(int eventId, int coastId, int count, int eventChartId,
            int listChartId)
        {
            var parameters = new List<KeyValuePair<string, object>>()
            {
                new(nameof(eventId), eventId),
                new(nameof(coastId), coastId),
                new(nameof(eventChartId), eventChartId),
                new(nameof(listChartId), listChartId)
            };

            return InvokeFunc($"SummonX{count}", parameters, ConvertFunctionResponse<SummonResult>());
        }

        public static UniTask<Response<StageCleared>> SetStageClear(int stageChartId, int levelChartId, int itemChartId,
            int statusChartId, int stage)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(stageChartId), stageChartId),
                new(nameof(levelChartId), levelChartId),
                new(nameof(itemChartId), itemChartId),
                new(nameof(statusChartId), statusChartId),
                new(nameof(stage), stage),
            };

            return InvokeFunc("StageClear", parameters, ConvertFunctionResponse<StageCleared>());
        }

        public static UniTask<Response<Inventory>> TEST_AddItems(int[] ids, int[] quantities)
        {
            var parameters = new List<KeyValuePair<string, object>>
            {
                new(nameof(ids), ids),
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

        private static T FromTransaction<T>(JsonData data, string tableName)
        {
            JsonData result = null;
            var responses = data[0];
            for (int i = 0, count = responses.Count; i < count && result == null; ++i)
            {
                var element = responses[i];
                if (element.ContainsKey(tableName))
                    result = element[tableName];
            }

            return result != null ? result.Cast<T>() : default;
        }

        private static UniTask<Response> InvokeFunc(string functionName,
            List<KeyValuePair<string, object>> parameters)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }

        private static UniTask<Response<T>> InvokeFunc<T>(string functionName,
            List<KeyValuePair<string, object>> parameters, Response<T>.Convert convert)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke), convert);
        }

        private static Response<T>.Convert ConvertFunctionResponse<T>() =>
            raw => raw.GetFlattenJSON()["result"].Cast<T>();

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

        private static async UniTask<Response> Call(Api api)
        {
            var src = new UniTaskCompletionSource<Response>();
            api.Invoke(result =>
            {
                --LeftRequestCount;

                var response = new Response(result);
                src.TrySetResult(response);
            });

            ++LeftRequestCount;

            return await src.Task;
        }

        private static UniTask<Response<T>> Call<T>(Api api, Response<T>.Convert convert = null)
        {
            var src = new UniTaskCompletionSource<Response<T>>();
            api.Invoke(result =>
            {
                --LeftRequestCount;

                var response = new Response<T>(result, convert);
                src.TrySetResult(response);
            });

            ++LeftRequestCount;

            return src.Task;
        }
    }
}