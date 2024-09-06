using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Network.Shared;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public class Stamina : IDisposable
    {
        public readonly ReactiveProperty<int> point;
        public readonly ReactiveProperty<int> pointLimit;
        public readonly ReactiveProperty<DateTime> lastUpdate;

        public const int INTERVAL = 10;
        public const int PER_ONCE = 1;

        private IDisposable _update;
        private CancellationTokenSource _ctSource;

        public Stamina(StaminaDto dto)
        {
            point = new(dto.point);
            pointLimit = new(dto.pointLimit);
            lastUpdate = new(dto.lastUpdate);

            RunLocalUpdate();
        }

        public void Update(StaminaDto dto)
        {
            point.Value = dto.point;
            pointLimit.Value = dto.pointLimit;
            lastUpdate.Value = dto.lastUpdate;

            RunLocalUpdate();
        }

        public void Dispose()
        {
            _update?.Dispose();
            _ctSource?.Cancel();
            point?.Dispose();
            pointLimit?.Dispose();
            lastUpdate?.Dispose();
        }
        private void RunLocalUpdate()
        {
            _ctSource?.Cancel();
            _ctSource = new();

            LocalUpdate(_ctSource.Token).Forget();
        }

        private DateTime Now => DateTime.UtcNow.AddHours(3);

        private async UniTaskVoid LocalUpdate(CancellationToken token)
        {
            try
            {
                while (point.Value < pointLimit.Value && !token.IsCancellationRequested)
                {
                    var nextUpdate = lastUpdate.Value.AddMinutes(INTERVAL);
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
            if (point.Value < pointLimit.Value)
            {
                var now = Now;
                int cycle = (int)((now - lastUpdate.Value).TotalMinutes / INTERVAL);
                if (cycle > 0)
                {
                    int amount = cycle * PER_ONCE;
                    point.Value = Mathf.Min(point.Value + amount, pointLimit.Value);
                    lastUpdate.Value = now;
                }
            }
        }
    }

    public class Currency : IDisposable
    {
        public readonly ReactiveProperty<int> paidDia;
        public readonly ReactiveProperty<int> freeDia;
        public readonly ReactiveProperty<int> gold;

        public Currency(CurrencyDto dto)
        {
            paidDia = new(dto.paidDia);
            freeDia = new(dto.freeDia);
            gold = new(dto.gold);
        }

        public void Update(CurrencyDto dto)
        {
            paidDia.Value = dto.paidDia;
            freeDia.Value = dto.freeDia;
            gold.Value = dto.gold;
        }

        public void Add(CurrencyDto dto)
        {
            paidDia.Value += dto.paidDia;
            freeDia.Value += dto.freeDia;
            gold.Value += dto.gold;
        }

        public void Dispose()
        {
            paidDia?.Dispose();
            freeDia?.Dispose();
            gold?.Dispose();
        }
    }

    public class Inventory : IDisposable
    {
        public readonly ReactiveCollection<IItem> items;

        public Inventory(InventoryDto dto) => items = new(dto.items);

        public void Update(InventoryDto dto) => items.Update(dto.items);

        public void Update(IItem item)
        {
            if (item.Quantity > 0)
                items.Update(item, x => x.ItemId == item.ItemId);
            else
                items.Update(null, x => x.ItemId == item.ItemId);
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
            var exist = items.FirstOrDefault(x => x.ItemId == item.ItemId);
            if (exist != null)
            {
                item.Quantity += exist.Quantity;
                Update(item);
            }
            else
                items.Add(item);
        }

        public IDisposable WhenUpdate<T>(ReactiveProperty<T> origin, Action<T> action) where T : IItem
        {
            return items.ChangeAsObservable()
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
            return items.ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(action.Invoke);
        }

        public void Dispose()
        {
            items?.Dispose();
        }
    }

    public class Characters : IDisposable
    {
        public readonly ReactiveCollection<UnitInfo> units;

        public Characters(CharactersDto dto) => units = new(dto.units);

        public void Update(CharactersDto dto) => units.Update(dto.units);

        public void Add(UnitInfo unit)
        {
            if (units.FirstOrDefault(x => x.id == unit.id) != null)
                return;

            units.Add(unit);
        }

        public IDisposable WhenUpdate(ReactiveProperty<UnitInfo> origin, Action<UnitInfo> action)
        {
            return units.ChangeAsObservable()
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
            return units.ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(action.Invoke);
        }

        public void Update(UnitInfo unit) => units.Update(unit, x => x.id == unit.id);

        public void Dispose()
        {
            units?.Dispose();
        }
    }

    public class Formation : IDisposable
    {
        public readonly ReactiveCollection<FieldUnit> fieldUnits;

        public Formation(FormationDto dto) => fieldUnits = new(dto.fieldUnits);

        public void Set(IEnumerable<UnitBehaviour> units)
        {
            fieldUnits.Clear();
            foreach (var unit in units)
            {
                var position = unit.position;
                fieldUnits.Add(new()
                {
                    id = unit.Id,
                    x = position.x,
                    y = position.y,
                });
            }
        }

        public void Update(FormationDto dto) => fieldUnits.Update(dto.fieldUnits);

        public void Dispose()
        {
            fieldUnits?.Dispose();
        }
    }

    public class GameRecord : IDisposable
    {
        public readonly ReactiveProperty<int> iconId;
        public readonly ReactiveProperty<int> castleLv;
        public readonly ReactiveProperty<int> lastClearedStage;
        public readonly ReactiveCollection<DungeonRecord> dungeon;

        public GameRecord(GameRecordDto dto)
        {
            iconId = new(dto.iconId);
            castleLv = new(dto.castleLv);
            lastClearedStage = new(dto.lastClearedStage);
            dungeon = new(dto.dungeon);
        }

        public void Update(GameRecordDto dto)
        {
            iconId.Value = dto.iconId;
            castleLv.Value = dto.castleLv;
            lastClearedStage.Value = dto.lastClearedStage;
            dungeon.Update(dto.dungeon);
        }

        public void Dispose()
        {
            iconId?.Dispose();
            castleLv?.Dispose();
            lastClearedStage?.Dispose();
            dungeon?.Dispose();
        }
    }

    public class ShopRecord : IDisposable
    {
        public readonly ReactiveCollection<Product> products;
        public readonly ReactiveCollection<ShopRecordDto.History> histories;

        public ShopRecord(ShopRecordDto dto)
        {
            products = new(dto.products);
            histories = new(dto.histories);
        }

        public void Update(ShopRecordDto dto)
        {
            products.Update(dto.products);
            histories.Update(dto.histories);
        }

        public void Dispose()
        {
            products?.Dispose();
            histories?.Dispose();
        }
    }
}