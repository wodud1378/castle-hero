using System;
using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
using CastleHero.Data.Repositories;
using CastleHero.Network.Service;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;

namespace CastleHero.Network.Impl.Local
{
    /// <summary>
    /// Local 네트워크 서비스 공통 베이스. 인메모리 UserDataDto 접근, 저장, 스태미나 회복 처리.
    /// Backend 의 BackendNetworkServiceBase 와 동일한 역할 (서버 RPC 대신 PlayerPrefs 동기화).
    /// </summary>
    public abstract class LocalNetworkServiceBase
    {
        protected static readonly ItemGenerator ItemGen = new();
        protected static readonly UnitGenerator UnitGen = new();

        // 구현체 서비스는 BeforeSceneLoad 에 생성되지만, IDBProvider/IUserRepository 는
        // 로그인 이후 등록되므로 ctor 시점에 미등록 상태. OnRegistered 구독으로 늦게 채움.
        protected IDBProvider Db { get; private set; }
        protected IUserRepository UserRepo { get; private set; }

        protected readonly LocalUserDataStore Store;

        protected LocalNetworkServiceBase(IServiceLocator sl, LocalUserDataStore store)
        {
            Store = store;
            if (sl.TryGet<IDBProvider>(out var db)) Db = db;
            if (sl.TryGet<IUserRepository>(out var userRepo)) UserRepo = userRepo;
            sl.OnRegistered += OnServiceRegistered;
        }

        private void OnServiceRegistered(Type type, object instance)
        {
            if (type == typeof(IDBProvider)) Db = (IDBProvider)instance;
            else if (type == typeof(IUserRepository)) UserRepo = (IUserRepository)instance;
        }

        protected Result<UserDataDto> Get()
        {
            var data = Store.Load();
            return data == null
                ? Result<UserDataDto>.Error(Error.DataNotFound)
                : Result<UserDataDto>.Complete(data);
        }

        protected void Save(UserDataDto data)
        {
            Store.Save(data);
            UserRepo?.Update(data);
        }

        protected static void RecoverStamina(StaminaDto stamina)
        {
            const int intervalMinute = 10;
            const int amountPerMinute = 1;

            if (stamina.point >= stamina.pointLimit)
                return;

            var now = ServerTime.Now;
            int cycle = (int)((now - stamina.lastUpdate).TotalMinutes / intervalMinute);
            if (cycle <= 0)
                return;

            int amount = cycle * amountPerMinute;
            stamina.point = Mathf.Min(stamina.point + amount, stamina.pointLimit);
            stamina.lastUpdate = now;
        }

        protected static bool HasEnoughAp(StaminaDto stamina, int point)
        {
            RecoverStamina(stamina);
            return stamina.point >= point;
        }
    }
}
