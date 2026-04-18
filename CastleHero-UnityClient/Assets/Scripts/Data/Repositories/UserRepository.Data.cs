using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.Common.Behaviours;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using UnityEngine;

namespace CastleHero.Data.Repositories
{
    public class Stamina : IDisposable
    {
        public readonly ReactiveProperty<int> Point;
        public readonly ReactiveProperty<int> PointLimit;
        public readonly ReactiveProperty<DateTime> LastUpdate;

        public const int INTERVAL = 10;
        public const int PER_ONCE = 1;

        private CancellationTokenSource _ctSource;

        public Stamina(StaminaDto dto)
        {
            Point = new(dto.point);
            PointLimit = new(dto.pointLimit);
            LastUpdate = new(dto.lastUpdate);

            RunLocalUpdate();
        }

        public void Update(StaminaDto dto)
        {
            Point.Value = dto.point;
            PointLimit.Value = dto.pointLimit;
            LastUpdate.Value = dto.lastUpdate;

            RunLocalUpdate();
        }

        public void Dispose()
        {
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            Point?.Dispose();
            PointLimit?.Dispose();
            LastUpdate?.Dispose();
        }

        private void RunLocalUpdate()
        {
            // 이전 토큰을 취소하고 새 토큰으로 재시작
            // Cancel() 후 새 Task를 즉시 시작해도 이전 Task는
            // SuppressCancellationThrow()로 감싸져 있어 안전하게 종료됨
            _ctSource?.Cancel();
            _ctSource?.Dispose();
            _ctSource = new CancellationTokenSource();

            LocalUpdate(_ctSource.Token).SafeForget();
        }

        private DateTime Now => ServerTime.Now;

        private async UniTaskVoid LocalUpdate(CancellationToken token)
        {
            try
            {
                while (Point.Value < PointLimit.Value && !token.IsCancellationRequested)
                {
                    var nextUpdate = LastUpdate.Value.AddMinutes(INTERVAL);
                    var now = Now;
                    var totalMs = (nextUpdate - now).TotalMilliseconds;
                    if (totalMs > 0)
                        await UniTask
                            .Delay((int)totalMs, true, cancellationToken: token)
                            .SuppressCancellationThrow();

                    await UniTask.SwitchToMainThread(cancellationToken: token);

                    UpdateValues();

                    await UniTask.SwitchToThreadPool();
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void UpdateValues()
        {
            if (Point.Value < PointLimit.Value)
            {
                var now = Now;
                int cycle = (int)((now - LastUpdate.Value).TotalMinutes / INTERVAL);
                if (cycle > 0)
                {
                    int amount = cycle * PER_ONCE;
                    Point.Value = Mathf.Min(Point.Value + amount, PointLimit.Value);
                    LastUpdate.Value = now;
                }
            }
        }
    }

    public class Currency : IDisposable
    {
        public readonly ReactiveProperty<int> PaidDia;
        public readonly ReactiveProperty<int> FreeDia;
        public readonly ReactiveProperty<int> Gold;

        public Currency(CurrencyDto dto)
        {
            PaidDia = new(dto.paidDia);
            FreeDia = new(dto.freeDia);
            Gold = new(dto.gold);
        }

        public void Update(CurrencyDto dto)
        {
            PaidDia.Value = dto.paidDia;
            FreeDia.Value = dto.freeDia;
            Gold.Value = dto.gold;
        }

        public void Add(CurrencyDto dto)
        {
            PaidDia.Value += dto.paidDia;
            FreeDia.Value += dto.freeDia;
            Gold.Value += dto.gold;
        }

        public void Dispose()
        {
            PaidDia?.Dispose();
            FreeDia?.Dispose();
            Gold?.Dispose();
        }
    }

    public class Inventory : IDisposable
    {
        public readonly ReactiveCollection<IItem> Items;

        public Inventory(InventoryDto dto) => Items = new(dto.items);

        public void Update(InventoryDto dto) => Items.Update(dto.items);

        public void Update(IItem item)
        {
            if (item.Quantity > 0)
                Items.Update(item, x => x.ItemId == item.ItemId);
            else
                Items.Update(null, x => x.ItemId == item.ItemId);
        }

        public void Add(IEnumerable<IItem> enumerable)
        {
            foreach (var item in enumerable)
            {
                Add(item);
            }
        }

        public void Add(IItem item)
        {
            var exist = Items.FirstOrDefault(x => x.ItemId == item.ItemId);
            if (exist != null)
            {
                item.Quantity += exist.Quantity;
                Update(item);
            }
            else
                Items.Add(item);
        }

        public IDisposable WhenUpdate<T>(ReactiveProperty<T> origin, Action<T> action) where T : IItem
        {
            return Items.ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x =>
                {
                    action.Invoke(origin.Value != null
                        ? x.OfType<T>().FirstOrDefault(item => item.ItemId == origin.Value.ItemId)
                        : default);
                });
        }

        public IDisposable WhenUpdate(Action<ReactiveCollection<IItem>> action)
        {
            return Items.ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(action.Invoke);
        }

        public void Dispose()
        {
            Items?.Dispose();
        }
    }

    public class Characters : IDisposable
    {
        public readonly ReactiveCollection<UnitInfo> Units;

        public Characters(CharactersDto dto) => Units = new(dto.units);

        public void Update(CharactersDto dto) => Units.Update(dto.units);

        public void Add(UnitInfo unit)
        {
            if (Units.FirstOrDefault(x => x.id == unit.id) != null)
                return;

            Units.Add(unit);
        }

        public IDisposable WhenUpdate(ReactiveProperty<UnitInfo> origin, Action<UnitInfo> action)
        {
            return Units.ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(x =>
                {
                    action.Invoke(origin.Value != null
                        ? x.FirstOrDefault(unit => unit.id == origin.Value.id)
                        : null);
                });
        }

        public IDisposable WhenUpdate(Action<ReactiveCollection<UnitInfo>> action)
        {
            return Units.ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(action.Invoke);
        }

        public void Update(UnitInfo unit) => Units.Update(unit, x => x.id == unit.id);

        public void Dispose()
        {
            Units?.Dispose();
        }
    }

    public class Formation : IDisposable
    {
        public readonly ReactiveCollection<FieldUnit> FieldUnits;

        public Formation(FormationDto dto) => FieldUnits = new(dto.fieldUnits);

        public void Set(IEnumerable<IUnitActor> units)
        {
            FieldUnits.Clear();
            foreach (var unit in units)
            {
                var position = unit.Position;
                FieldUnits.Add(new()
                {
                    id = unit.Id,
                    x = position.x,
                    y = position.y,
                });
            }
        }

        public void Update(FormationDto dto) => FieldUnits.Update(dto.fieldUnits);

        public void Dispose()
        {
            FieldUnits?.Dispose();
        }
    }

    public class GameRecord : IDisposable
    {
        public readonly ReactiveProperty<int> IconId;
        public readonly ReactiveProperty<int> CastleLv;
        public readonly ReactiveProperty<int> LastClearedStage;
        public readonly ReactiveCollection<DungeonRecord> Dungeon;

        public GameRecord(GameRecordDto dto)
        {
            IconId = new(dto.iconId);
            CastleLv = new(dto.castleLv);
            LastClearedStage = new(dto.lastClearedStage);
            Dungeon = new(dto.dungeon);
        }

        public void Update(GameRecordDto dto)
        {
            IconId.Value = dto.iconId;
            CastleLv.Value = dto.castleLv;
            LastClearedStage.Value = dto.lastClearedStage;
            Dungeon.Update(dto.dungeon);
        }

        public void Dispose()
        {
            IconId?.Dispose();
            CastleLv?.Dispose();
            LastClearedStage?.Dispose();
            Dungeon?.Dispose();
        }
    }

    public class ShopRecord : IDisposable
    {
        public readonly ReactiveCollection<Product> Products;
        public readonly ReactiveCollection<ShopRecordDto.History> Histories;

        public ShopRecord(ShopRecordDto dto)
        {
            Products = new(dto.products);
            Histories = new(dto.histories);
        }

        public void Update(ShopRecordDto dto)
        {
            Products.Update(dto.products);
            Histories.Update(dto.histories);
        }

        public void Dispose()
        {
            Products?.Dispose();
            Histories?.Dispose();
        }
    }
}
