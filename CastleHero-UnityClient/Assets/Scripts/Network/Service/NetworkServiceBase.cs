using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Data;
using RGLabs.Data.Repositories;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RGLabs.Network.Service
{
    public enum Table
    {
        Act,
        Currency,
        Character,
        Formation,
        Inventory,
        GameRecord,
        ShopRecord,
    }

    public interface IErrorHandler
    {
        public void OnError(string error);

        public void OnError(Error code);
    }

    public class Result<T>
    {
        public bool IsSuccess => error == Error.None;

        public T data;
        public Error error;
        public string errorMessage;

        public static Result<T> From(T data)
        {
            return new Result<T>
            {
                data = data,
                error = Error.None
            };
        }

        public static Result<T> FromError(Error error) => FromError(error, string.Empty);

        public static Result<T> FromError(Error error, string errorMessage) =>
            new() { error = error, errorMessage = errorMessage };
    }

    public abstract class NetworkServiceBase
    {
        protected delegate void Api(Backend.BackendCallback onResult);

        protected static readonly ItemGenerator ItemGen = new();
        protected static readonly UnitGenerator UnitGen = new();

        protected static readonly Dictionary<Table, string> TableNames = new()
        {
            { Table.Act, "act" },
            { Table.Currency, "currency" },
            { Table.Character, "characters" },
            { Table.Formation, "formation" },
            { Table.Inventory, "inventory" },
            { Table.GameRecord, "record" },
            { Table.ShopRecord, "shop" },
        };

        public static int LeftRequestCount { get; private set; }

        private readonly List<IErrorHandler> _errorHandlers = new();

        public void AttachErrorHandler(IErrorHandler handler)
        {
            if (!_errorHandlers.Contains(handler))
                return;

            _errorHandlers.Add(handler);
        }

        public void DetachErrorHandler(IErrorHandler handler) => _errorHandlers.Remove(handler);

        protected void PublishError(Error error) => _errorHandlers.ForEach(x => x.OnError(error));

        protected void PublishError(string error) => _errorHandlers.ForEach(x => x.OnError(error));


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
                    if (response.error != Error.None)
                        PublishError(response.error);

                    src.TrySetResult(response);
                }
                catch (Exception e)
                {
                    var exception = e.ToString();

                    PublishError(e.ToString());

                    Debug.LogError(exception);
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

        protected async UniTask<Result<T>> GetTable<T>(Table table) where T : class
        {
            string name = TableNames[table];

            var response = await Call(onResult => Backend.PlayerData.GetMyData(name, 1, onResult.Invoke),
                raw =>
                {
                    var jsonData = raw.FlattenRows();
                    return jsonData.ContainsKey(name)
                        ? jsonData[name].Cast<T>()
                        : null;
                });

            return response.IsSuccess
                ? Result<T>.From(response.data)
                : Result<T>.FromError(response.error);
        }

        protected async UniTask<Result<UserDataDto>> GetTables(params Table[] tables)
        {
            var read = new PlayerDataTransactionRead();

            var list = tables == null
                ? TableNames.Keys.ToList()
                : tables.ToList();

            foreach (var table in list)
            {
                read.AddGetMyLatestData(TableNames[table]);
            }

            var response = await Call(onResult =>
                    Backend.PlayerData.TransactionRead(read, onResult.Invoke),
                raw =>
                {
                    var jsonData = raw.GetFlattenJSON();
                    return new UserDataDto
                    {
                        act = FromTransaction<ActDto>(jsonData, TableNames[Table.Act]),
                        currency = FromTransaction<CurrencyDto>(jsonData, TableNames[Table.Currency]),
                        characters = FromTransaction<CharactersDto>(jsonData, TableNames[Table.Character]),
                        formation = FromTransaction<FormationDto>(jsonData, TableNames[Table.Formation]),
                        inventory = FromTransaction<InventoryDto>(jsonData, TableNames[Table.Inventory]),
                        gameRecord = FromTransaction<GameRecordDto>(jsonData, TableNames[Table.GameRecord]),
                        shopRecord = FromTransaction<ShopRecordDto>(jsonData, TableNames[Table.ShopRecord]),
                    };
                });

            return response.IsSuccess
                ? Result<UserDataDto>.From(response.data)
                : Result<UserDataDto>.FromError(response.error);
        }

        protected async UniTask<Result<UserDataDto>> GetTables(IEnumerable<Table> tables)
        {
            var read = new PlayerDataTransactionRead();

            var list = tables == null
                ? TableNames.Keys.ToList()
                : tables.ToList();

            foreach (var table in list)
            {
                read.AddGetMyLatestData(TableNames[table]);
            }

            var response = await Call(onResult =>
                    Backend.PlayerData.TransactionRead(read, onResult.Invoke),
                raw =>
                {
                    var jsonData = raw.GetFlattenJSON();
                    return new UserDataDto
                    {
                        act = FromTransaction<ActDto>(jsonData, TableNames[Table.Act]),
                        currency = FromTransaction<CurrencyDto>(jsonData, TableNames[Table.Currency]),
                        characters = FromTransaction<CharactersDto>(jsonData, TableNames[Table.Character]),
                        formation = FromTransaction<FormationDto>(jsonData, TableNames[Table.Formation]),
                        inventory = FromTransaction<InventoryDto>(jsonData, TableNames[Table.Inventory]),
                        gameRecord = FromTransaction<GameRecordDto>(jsonData, TableNames[Table.GameRecord]),
                        shopRecord = FromTransaction<ShopRecordDto>(jsonData, TableNames[Table.ShopRecord]),
                    };
                });

            return response.IsSuccess
                ? Result<UserDataDto>.From(response.data)
                : Result<UserDataDto>.FromError(response.error);
        }

        protected async UniTask<Response> UpdateTable(Table key, object value)
        {
            var name = TableNames[key];
            var param = ToParam(name, value);

            return await Call(onResult =>
                Backend.PlayerData.UpdateMyLatestData(name, param, onResult.Invoke));
        }

        protected UniTask<Result<bool>> UpdateTables(UserDataDto userData, bool updateStorage = true)
        {
            var tables = new Dictionary<Table, object>();
            if (userData.act != null)
                tables.Add(Table.Act, userData.act);

            if (userData.currency != null)
                tables.Add(Table.Currency, userData.currency);

            if (userData.currency != null)
                tables.Add(Table.Inventory, userData.inventory);

            if (userData.characters != null)
                tables.Add(Table.Character, userData.characters);

            if (userData.formation != null)
                tables.Add(Table.Formation, userData.formation);

            if (userData.gameRecord != null)
                tables.Add(Table.GameRecord, userData.gameRecord);

            if (userData.shopRecord != null)
                tables.Add(Table.ShopRecord, userData.shopRecord);

            return UpdateTables(tables, updateStorage);
        }

        protected async UniTask<Result<bool>> UpdateTables(Dictionary<Table, object> tables, bool updateStorage = true)
        {
            var write = new PlayerDataTransactionWrite();
            foreach (var table in tables)
            {
                var name = TableNames[table.Key];
                var param = ToParam(name, table.Value);
                write.AddUpdateMyLatestData(name, param);
            }

            var writeResponse = await Call(onResult =>
                Backend.PlayerData.TransactionWrite(write, onResult.Invoke));

            if (!writeResponse.IsSuccess)
                return Result<bool>.FromError(writeResponse.error);

            var readResponse = await GetTables();
            if (!readResponse.IsSuccess)
                return Result<bool>.FromError(writeResponse.error);

            if (updateStorage)
                Storage.userRepository.Update(readResponse.data);

            return Result<bool>.From(true);
        }

        protected async UniTask<Result<bool>> HasEnoughAp(ActDto act, int point)
        {
            var update = await UpdateAct(act);
            if (!update.IsSuccess)
                return Result<bool>.FromError(update.error);

            return Result<bool>.From(update.data.point >= point);
        }

        protected async UniTask<Result<ActDto>> UpdateAct(ActDto act)
        {
            const int intervalMinute = 10;
            const int amountPerMinute = 1;

            if (act.pointLimit > act.point)
            {
                var serverTime = await GetServerTime();
                if (!serverTime.IsSuccess)
                    return Result<ActDto>.FromError(serverTime.error);

                var now = serverTime.data;
                var minutes = (now - act.lastUpdate).TotalMinutes;
                int amount = (int)minutes / (intervalMinute * amountPerMinute);
                int total = act.point + amount;

                if (total != act.point)
                {
                    act.point = Mathf.Min(total, act.pointLimit);
                    act.lastUpdate = now;
                }
            }

            return Result<ActDto>.From(act);
        }

        protected async UniTask<Result<ActDto>> UpdateAct()
        {
            var read = await GetTable<ActDto>(Table.Act);
            if (!read.IsSuccess)
                return Result<ActDto>.FromError(read.error);

            return await UpdateAct(read.data);
        }

        private async UniTask<Result<DateTime>> GetServerTime()
        {
            var getServerTime = await Call(
                Backend.Utils.GetServerTime,
                raw => DateTime.Parse(raw.GetFlattenJSON()["utcTime"].ToString()));

            if (!getServerTime.IsSuccess)
                return Result<DateTime>.FromError(getServerTime.error);

            return Result<DateTime>.From(getServerTime.data.AddHours(3));
        }

        protected Param ToParam(string key, object value) => new() { { key, value.ToJson() } };
    }
}