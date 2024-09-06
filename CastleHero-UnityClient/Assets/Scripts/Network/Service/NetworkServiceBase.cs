using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using LitJson;
using RGLabs.Data;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace RGLabs.Network.Service
{
    public enum Table
    {
        Stamina,
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

    public class Result
    {
        public bool IsSuccess => error == Network.Error.None;

        public bool fromBackend;
        public int statusCode;
        public Error error;
        public string errorMessage;
        public BackendReturnObject raw;

        public static Result Complete(BackendReturnObject raw)
        {
            return new Result
            {
                fromBackend = true,
                statusCode = int.Parse(raw.GetStatusCode()),
                error = ToError(raw),
                raw = raw
            };
        }

        public static Result Complete() => new() { error = Network.Error.None };

        public static Result Error(Error error) => Error(error, string.Empty);

        public static Result Error(Error error, string errorMessage) =>
            new() { error = error, errorMessage = errorMessage };

        protected static Error ToError(BackendReturnObject obj) =>
            obj.IsSuccess()
                ? Network.Error.None
                : obj.GetErrorCode() switch
                {
                    "NetworkError" => Network.Error.FromNetwork,
                    "UnauthorizedException" => Network.Error.Unauthorized,
                    "ServerException" => Network.Error.FromServer,
                    "Maintenance" => Network.Error.Maintenance,
                    _ => Network.Error.Unknown
                };
    }

    public class Result<T> : Result
    {
        public delegate T Convert(BackendReturnObject jsonData);

        public T data;

        public static Result<T> Complete(T data)
        {
            return new Result<T>
            {
                data = data,
                error = Network.Error.None
            };
        }

        public static Result<T> Complete(BackendReturnObject raw, Convert convert = null)
        {
            var result = new Result<T>
            {
                fromBackend = true,
                error = ToError(raw),
                statusCode = int.Parse(raw.GetStatusCode()),
                raw = raw
            };

            if (!result.IsSuccess)
                return result;

            if (convert != null)
            {
                result.data = convert.Invoke(raw);
            }
            else
            {
                var rows = raw.FlattenRows();
                var json = JsonMapper.ToJson(rows);
                result.data = JsonMapper.ToObject<T>(json);
            }

            return result;
        }

        public new static Result<T> Error(Error error) => Error(error, string.Empty);

        public new static Result<T> Error(Error error, string errorMessage) =>
            new() { error = error, errorMessage = errorMessage };
    }

    public abstract class NetworkServiceBase
    {
        protected delegate void Api(Backend.BackendCallback onResult);

        protected static readonly ItemGenerator ItemGen = new();
        protected static readonly UnitGenerator UnitGen = new();

        protected static readonly Dictionary<Table, string> TableNames = new()
        {
            { Table.Stamina, "stamina" },
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

        protected async UniTask<Result> Call(Api api)
        {
            var src = new UniTaskCompletionSource<Result>();
            api.Invoke(raw =>
            {
                --LeftRequestCount;

                try
                {
                    var result = Result.Complete(raw);
                    if (!result.IsSuccess)
                        PublishError(result.error);

                    src.TrySetResult(result);
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

        protected UniTask<Result<T>> Call<T>(Api api, Result<T>.Convert convert = null)
        {
            var src = new UniTaskCompletionSource<Result<T>>();
            api.Invoke(raw =>
            {
                --LeftRequestCount;

                try
                {
                    var result = Result<T>.Complete(raw, convert);
                    src.TrySetResult(result);
                }
                catch (Exception e)
                {
                    src.TrySetResult(Result<T>.Error(Error.Unknown, e.ToString()));
#if UNITY_EDITOR
                    Debug.LogError(e.ToString());
#endif
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
                    var jsonData = raw.FlattenRows()[0];
                    return jsonData.ContainsKey(name)
                        ? jsonData[name].Cast<T>()
                        : null;
                });

            return response.IsSuccess
                ? Result<T>.Complete(response.data)
                : Result<T>.Error(response.error);
        }

        protected async UniTask<Result<UserDataDto>> GetTables(params Table[] tables)
        {
            var read = new PlayerDataTransactionRead();

            var list = tables.Length == 0
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
                        stamina = FromTransaction<StaminaDto>(jsonData, TableNames[Table.Stamina]),
                        currency = FromTransaction<CurrencyDto>(jsonData, TableNames[Table.Currency]),
                        characters = FromTransaction<CharactersDto>(jsonData, TableNames[Table.Character]),
                        formation = FromTransaction<FormationDto>(jsonData, TableNames[Table.Formation]),
                        inventory = FromTransaction<InventoryDto>(jsonData, TableNames[Table.Inventory]),
                        gameRecord = FromTransaction<GameRecordDto>(jsonData, TableNames[Table.GameRecord]),
                        shopRecord = FromTransaction<ShopRecordDto>(jsonData, TableNames[Table.ShopRecord]),
                    };
                });

            return response.IsSuccess
                ? Result<UserDataDto>.Complete(response.data)
                : Result<UserDataDto>.Error(response.error);
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
                        stamina = FromTransaction<StaminaDto>(jsonData, TableNames[Table.Stamina]),
                        currency = FromTransaction<CurrencyDto>(jsonData, TableNames[Table.Currency]),
                        characters = FromTransaction<CharactersDto>(jsonData, TableNames[Table.Character]),
                        formation = FromTransaction<FormationDto>(jsonData, TableNames[Table.Formation]),
                        inventory = FromTransaction<InventoryDto>(jsonData, TableNames[Table.Inventory]),
                        gameRecord = FromTransaction<GameRecordDto>(jsonData, TableNames[Table.GameRecord]),
                        shopRecord = FromTransaction<ShopRecordDto>(jsonData, TableNames[Table.ShopRecord]),
                    };
                });

            return response.IsSuccess
                ? Result<UserDataDto>.Complete(response.data)
                : Result<UserDataDto>.Error(response.error);
        }

        protected async UniTask<Result> UpdateTable(Table key, object value)
        {
            var name = TableNames[key];
            var param = ToParam(name, value);

            return await Call(onResult =>
                Backend.PlayerData.UpdateMyLatestData(name, param, onResult.Invoke));
        }

        protected UniTask<Result> UpdateTables(UserDataDto userData, bool updateStorage = true)
        {
            var tables = new Dictionary<Table, object>();
            if (userData.stamina != null)
                tables.Add(Table.Stamina, userData.stamina);

            if (userData.currency != null)
                tables.Add(Table.Currency, userData.currency);

            if (userData.inventory != null)
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

        protected async UniTask<Result> UpdateTables(Dictionary<Table, object> tables, bool updateStorage = true)
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
                return Result.Error(writeResponse.error);

            var readResponse = await GetTables();
            if (!readResponse.IsSuccess)
                return Result.Error(writeResponse.error);

            if (updateStorage)
                Storage.userRepository.Update(readResponse.data);

            return Result.Complete();
        }

        protected async UniTask<Result<bool>> HasEnoughAp(StaminaDto stamina, int point)
        {
            var update = await UpdateStamina(stamina);
            if (!update.IsSuccess)
                return Result<bool>.Error(update.error);

            return Result<bool>.Complete(stamina.point >= point);
        }

        protected async UniTask<Result> UpdateStamina(StaminaDto stamina)
        {
            const int intervalMinute = 10;
            const int amountPerMinute = 1;

            if (stamina.point < stamina.pointLimit)
            {
                var serverTime = await GetServerTime();
                if (!serverTime.IsSuccess)
                    return Result.Error(serverTime.error);

                var now = serverTime.data;
                int cycle = (int)((now - stamina.lastUpdate).TotalMinutes / intervalMinute);
                if (cycle > 0)
                {
                    int amount = cycle * amountPerMinute;
                    stamina.point = Mathf.Min(stamina.point + amount, stamina.pointLimit);
                    stamina.lastUpdate = now.AddMinutes(cycle * intervalMinute);
                }
            }

            return Result.Complete();
        }

        protected async UniTask<Result> UpdateStamina()
        {
            var read = await GetTable<StaminaDto>(Table.Stamina);
            if (!read.IsSuccess)
                return Result.Error(read.error);

            return await UpdateStamina(read.data);
        }

        protected async UniTask<Result> AddStamina(StaminaDto stamina, int amount)
        {
            var update = await UpdateStamina(stamina);
            if (!update.IsSuccess)
                return Result<StaminaDto>.Error(update.error);

            stamina.point += amount * amount;
            return Result.Complete();
        }

        protected async UniTask<Result<DateTime>> GetServerTime()
        {
            var getServerTime = await Call(
                Backend.Utils.GetServerTime,
                raw => DateTime.Parse(raw.GetFlattenJSON()["utcTime"].ToString()));

            if (!getServerTime.IsSuccess)
                return Result<DateTime>.Error(getServerTime.error);

            return Result<DateTime>.Complete(getServerTime.data.AddHours(3));
        }

        protected Param ToParam(string key, object value) => new() { { key, value.ToJson() } };
    }
}