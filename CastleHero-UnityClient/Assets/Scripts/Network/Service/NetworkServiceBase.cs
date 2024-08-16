using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Network.Service
{
    public abstract class NetworkServiceBase
    {
        protected delegate void Api(Backend.BackendCallback onResult);
        
        public static int LeftRequestCount { get; private set; }
        
        protected const string ACT_TABLE = "act";
        protected const string CURRENCY_TABLE = "currency";
        protected const string CHARACTERS_TABLE = "characters";
        protected const string FORMATION_TABLE = "formation";
        protected const string INVENTORY_TABLE = "inventory";
        protected const string GAME_RECORD_TABLE = "record";
        protected const string SHOP_RECORD_TABLE = "shop";
        
        protected UniTask Save(string tableName, object obj)
        {
            var param = new Param { { tableName, obj.ToJson() } };
            var api = new Api(onResult => { Backend.GameData.Update(tableName, new Where(), param, onResult.Invoke); });

            return Call(api);
        }
        
        protected UniTask<Response> InvokeFunc(string functionName,
            List<KeyValuePair<string, object>> parameters)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke));
        }

        protected UniTask<Response<T>> InvokeFunc<T>(string functionName,
            List<KeyValuePair<string, object>> parameters, Response<T>.Convert convert)
        {
            var param = FunctionParam(functionName, parameters);
            return Call(onResult => Backend.BFunc.InvokeFunction("function", param, onResult.Invoke), convert);
        }
        
        protected T FromTransaction<T>(JsonData data, string tableName)
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
        
        protected Response<T>.Convert ConvertFunctionResponse<T>() =>
            raw =>
            {
                var dto = raw.GetFlattenJSON()["result"].Cast<ResponseDto<T>>();
                var error = dto.error;
                if (!string.IsNullOrEmpty(error))
                {
                    Debug.LogError($"{error}, detail={dto.errorDetail}");
                    return default;
                }

                return dto.data;
            };
        
        protected Param FunctionParam(string functionName, List<KeyValuePair<string, object>> parameters = null)
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
        
        protected async UniTask<Response> Call(Api api)
        {
            var src = new UniTaskCompletionSource<Response>();
            api.Invoke(result =>
            {
                --LeftRequestCount;

                try
                {
                    var response = new Response(result);
                    src.TrySetResult(response);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    throw;
                }
            });

            ++LeftRequestCount;

            return await src.Task;
        }

        protected UniTask<Response<T>> Call<T>(Api api, Response<T>.Convert convert = null)
        {
            var src = new UniTaskCompletionSource<Response<T>>();
            api.Invoke(result =>
            {
                --LeftRequestCount;

                try
                {
                    var response = new Response<T>(result, convert);
                    src.TrySetResult(response);
                }
                catch (Exception e)
                {
                    Debug.LogError(e.ToString());
                    throw;
                }
            });

            ++LeftRequestCount;

            return src.Task;
        }
        
        protected List<TransactionValue> TransactionGet(params string[] tables)
        {
            var list = new List<TransactionValue>();
            if (tables.Contains(ACT_TABLE))
                list.Add(TransactionGetAct());

            if (tables.Contains(CURRENCY_TABLE))
                list.Add(TransactionGetCurrency());

            if (tables.Contains(CHARACTERS_TABLE))
                list.Add(TransactionGetCharacters());

            if (tables.Contains(FORMATION_TABLE))
                list.Add(TransactionGetFormation());

            if (tables.Contains(INVENTORY_TABLE))
                list.Add(TransactionGetInventory());
            
            if (tables.Contains(GAME_RECORD_TABLE))
                list.Add(TransactionGetGameRecord());
            
            if (tables.Contains(SHOP_RECORD_TABLE))
                list.Add(TransactionGetShopRecord());

            return list;
        }

        private TransactionValue TransactionGetShopRecord() => TransactionValue.SetGet(SHOP_RECORD_TABLE, new Where());
        
        private TransactionValue TransactionGetGameRecord() => TransactionValue.SetGet(GAME_RECORD_TABLE, new Where());

        private TransactionValue TransactionGetInventory() => TransactionValue.SetGet(INVENTORY_TABLE, new Where());

        private TransactionValue TransactionGetCharacters() => TransactionValue.SetGet(CHARACTERS_TABLE, new Where());

        private TransactionValue TransactionGetFormation() => TransactionValue.SetGet(FORMATION_TABLE, new Where());

        private TransactionValue TransactionGetCurrency() => TransactionValue.SetGet(CURRENCY_TABLE, new Where());

        private TransactionValue TransactionGetAct() => TransactionValue.SetGet(ACT_TABLE, new Where());
    }
}