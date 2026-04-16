using System;
using System.Collections.Generic;
using System.Linq;
using BackEnd;
using Cysharp.Threading.Tasks;
using LitJson;
using CastleHero.Data;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using Debug = UnityEngine.Debug;

using CastleHero.Common.Pattern;
using CastleHero.Data.Repositories;
namespace CastleHero.Network.Impl.Backend
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

    public interface IBackendErrorReceiver
    {
        public void OnError(string error);
        public void OnError(Error code);
    }

    public abstract class BackendNetworkServiceBase
    {
        protected delegate void Api(global::BackEnd.Backend.BackendCallback onResult);

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

        private readonly List<IBackendErrorReceiver> _errorHandlers = new();

        public void AttachErrorHandler(IBackendErrorReceiver handler)
        {
            if (_errorHandlers.Contains(handler))
                return;
            _errorHandlers.Add(handler);
        }

        public void DetachErrorHandler(IBackendErrorReceiver handler) => _errorHandlers.Remove(handler);

        protected void PublishError(Error error) => _errorHandlers.ForEach(x => x.OnError(error));
        protected void PublishError(string error) => _errorHandlers.ForEach(x => x.OnError(error));

        private static Error ToError(BackendReturnObject raw) =>
            raw.IsSuccess()
                ? Error.None
                : raw.GetErrorCode() switch
                {
                    "NetworkError" => Error.FromNetwork,
                    "UnauthorizedException" => Error.Unauthorized,
                    "ServerException" => Error.FromServer,
                    "Maintenance" => Error.Maintenance,
                    _ => Error.Unknown
                };

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

        protected async UniTask<BackendResult> Call(Api api)
        {
            var src = new UniTaskCompletionSource<BackendResult>();
            api.Invoke(raw =>
            {
                try
                {
                    var error = ToError(raw);
                    var result = error == Error.None
                        ? BackendResult.Complete(raw)
                        : BackendResult.Error(error);
                    result.statusCode = int.Parse(raw.GetStatusCode());
                    result.raw = raw;

                    if (!result.IsSuccess)
                        PublishError(result.error);

                    src.TrySetResult(result);
                }
                catch (Exception e)
                {
                    var msg = e.ToString();
                    PublishError(msg);
                    Debug.LogError(msg);
                    src.TrySetResult(BackendResult.Error(Error.Unknown, msg));
                }
            });

            return await src.Task;
        }

        protected UniTask<BackendResult<T>> Call<T>(Api api, Func<BackendReturnObject, T> convert = null)
        {
            var src = new UniTaskCompletionSource<BackendResult<T>>();
            api.Invoke(raw =>
            {
                try
                {
                    var error = ToError(raw);
                    if (error != Error.None)
                    {
                        var errResult = BackendResult<T>.Error(error);
                        errResult.statusCode = int.Parse(raw.GetStatusCode());
                        errResult.raw = raw;
                        src.TrySetResult(errResult);
                        return;
                    }

                    T data;
                    if (convert != null)
                    {
                        data = convert.Invoke(raw);
                    }
                    else
                    {
                        var rows = raw.FlattenRows();
                        var json = JsonMapper.ToJson(rows);
                        data = JsonMapper.ToObject<T>(json);
                    }

                    var result = BackendResult<T>.Complete(data, raw);
                    result.statusCode = int.Parse(raw.GetStatusCode());
                    src.TrySetResult(result);
                }
                catch (Exception e)
                {
                    src.TrySetResult(BackendResult<T>.Error(Error.Unknown, e.ToString()));
#if UNITY_EDITOR
                    Debug.LogError(e.ToString());
#endif
                }
            });

            return src.Task;
        }

        protected async UniTask<BackendResult<T>> GetTable<T>(Table table) where T : class
        {
            string name = TableNames[table];

            var response = await Call<T>(
                onResult => global::BackEnd.Backend.PlayerData.GetMyData(name, 1, onResult.Invoke),
                raw =>
                {
                    var jsonData = raw.FlattenRows()[0];
                    return jsonData.ContainsKey(name)
                        ? jsonData[name].Cast<T>()
                        : null;
                });

            return response.IsSuccess
                ? BackendResult<T>.Complete(response.data, response.raw)
                : BackendResult<T>.Error(response.error);
        }

        protected UniTask<BackendResult<UserDataDto>> GetTables(params Table[] tables)
            => GetTables(tables.Length == 0 ? null : (IEnumerable<Table>)tables);

        protected async UniTask<BackendResult<UserDataDto>> GetTables(IEnumerable<Table> tables = null)
        {
            var read = new PlayerDataTransactionRead();

            var list = tables == null
                ? TableNames.Keys.ToList()
                : tables.ToList();

            foreach (var table in list)
                read.AddGetMyLatestData(TableNames[table]);

            var response = await Call<UserDataDto>(
                onResult => global::BackEnd.Backend.PlayerData.TransactionRead(read, onResult.Invoke),
                raw =>
                {
                    var jsonData = raw.GetFlattenJSON();
                    return new UserDataDto
                    {
                        stamina    = FromTransaction<StaminaDto>(jsonData, TableNames[Table.Stamina]),
                        currency   = FromTransaction<CurrencyDto>(jsonData, TableNames[Table.Currency]),
                        characters = FromTransaction<CharactersDto>(jsonData, TableNames[Table.Character]),
                        formation  = FromTransaction<FormationDto>(jsonData, TableNames[Table.Formation]),
                        inventory  = FromTransaction<InventoryDto>(jsonData, TableNames[Table.Inventory]),
                        gameRecord = FromTransaction<GameRecordDto>(jsonData, TableNames[Table.GameRecord]),
                        shopRecord = FromTransaction<ShopRecordDto>(jsonData, TableNames[Table.ShopRecord]),
                    };
                });

            if (!response.IsSuccess)
            {
                var err = BackendResult<UserDataDto>.Error(response.error);
                err.statusCode = response.statusCode;
                return err;
            }

            return BackendResult<UserDataDto>.Complete(response.data, response.raw);
        }

        protected async UniTask<BackendResult> UpdateTable(Table key, object value)
        {
            var name = TableNames[key];
            var param = ToParam(name, value);

            return await Call(onResult =>
                global::BackEnd.Backend.PlayerData.UpdateMyLatestData(name, param, onResult.Invoke));
        }

        protected UniTask<BackendResult> UpdateTables(UserDataDto userData, bool updateStorage = true)
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

        protected async UniTask<BackendResult> UpdateTables(Dictionary<Table, object> tables, bool updateStorage = true)
        {
            var write = new PlayerDataTransactionWrite();
            foreach (var table in tables)
            {
                var name = TableNames[table.Key];
                var param = ToParam(name, table.Value);
                write.AddUpdateMyLatestData(name, param);
            }

            var writeResponse = await Call(onResult =>
                global::BackEnd.Backend.PlayerData.TransactionWrite(write, onResult.Invoke));

            if (!writeResponse.IsSuccess)
                return BackendResult.Error(writeResponse.error);

            var readResponse = await GetTables();
            if (!readResponse.IsSuccess)
                return BackendResult.Error(writeResponse.error);

            if (updateStorage)
                ServiceLocator.Get<IUserRepository>().Update(readResponse.data);

            return BackendResult.Complete(readResponse.raw);
        }

        protected async UniTask<BackendResult<bool>> HasEnoughAp(StaminaDto stamina, int point)
        {
            var update = await UpdateStamina(stamina);
            if (!update.IsSuccess)
                return BackendResult<bool>.Error(update.error);

            return BackendResult<bool>.Complete(stamina.point >= point, null);
        }

        protected async UniTask<BackendResult> UpdateStamina(StaminaDto stamina)
        {
            const int intervalMinute = 10;
            const int amountPerMinute = 1;

            if (stamina.point < stamina.pointLimit)
            {
                var serverTime = await GetServerTime();
                if (!serverTime.IsSuccess)
                    return BackendResult.Error(serverTime.error);

                var now = serverTime.data;
                int cycle = (int)((now - stamina.lastUpdate).TotalMinutes / intervalMinute);
                if (cycle > 0)
                {
                    int amount = cycle * amountPerMinute;
                    stamina.point = Mathf.Min(stamina.point + amount, stamina.pointLimit);
                    stamina.lastUpdate = now;
                }
            }

            return BackendResult.Complete(null);
        }

        protected async UniTask<BackendResult> UpdateStamina()
        {
            var read = await GetTable<StaminaDto>(Table.Stamina);
            if (!read.IsSuccess)
                return BackendResult.Error(read.error);

            var stamina = read.data;
            var result = await UpdateStamina(stamina);
            if (!result.IsSuccess)
                return BackendResult.Error(result.error);

            var update = await UpdateTable(Table.Stamina, stamina);
            if (!update.IsSuccess)
                return BackendResult.Error(update.error);

            ServiceLocator.Get<IUserRepository>().Stamina.Update(stamina);
            return BackendResult.Complete(update.raw);
        }

        protected async UniTask<BackendResult> AddStamina(StaminaDto stamina, int amount)
        {
            var update = await UpdateStamina(stamina);
            if (!update.IsSuccess)
                return BackendResult.Error(update.error);

            stamina.point += amount;
            return BackendResult.Complete(null);
        }

        protected async UniTask<BackendResult<DateTime>> GetServerTime()
        {
            var getServerTime = await Call<DateTime>(
                global::BackEnd.Backend.Utils.GetServerTime,
                raw => DateTime.Parse(raw.GetFlattenJSON()["utcTime"].ToString()));

            if (!getServerTime.IsSuccess)
                return BackendResult<DateTime>.Error(getServerTime.error);

            return BackendResult<DateTime>.Complete(ServerTime.ToServerTime(getServerTime.data), getServerTime.raw);
        }

        protected Param ToParam(string key, object value) => new() { { key, value.ToJson() } };
    }
}
