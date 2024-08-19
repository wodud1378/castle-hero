using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using RGLabs.Data.Model;
using RGLabs.Network.Service;
using RGLabs.Network.Shared;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public struct GameEntrance
    {
        public GameType type;
        public int id;
    }

    public class Act : IDisposable
    {
        // TODO : 행동력 계산 로직 UIAct에서 이 클래스로 이관
        private struct Calculation
        {
            public int point;
            public int pointLimit;
            public DateTime lastUpdate;
        }
        
        public readonly ReactiveProperty<int> point;
        public readonly ReactiveProperty<int> pointLimit;
        public readonly ReactiveProperty<DateTime> lastUpdate;

        private const int ApAddIntervalMinute = 10;
        private const int ApAddPerOnce = 1;

        private IDisposable _update;
        
        public Act(ActDto dto)
        {
            point = new(dto.point);
            pointLimit = new(dto.pointLimit);
            lastUpdate = new(dto.lastUpdate);
        }

        public void Update(ActDto dto)
        {
            point.Value = dto.point;
            pointLimit.Value = dto.pointLimit;
            lastUpdate.Value = dto.lastUpdate;
        }

        public void Dispose()
        {
            _update?.Dispose();
            point?.Dispose();
            pointLimit?.Dispose();
            lastUpdate?.Dispose();
        }
        
        private async UniTaskVoid UpdateTask()
        {
            while (true)
            {
                // 계산은 스레드 풀에서 진행.
                var result = await UniTask.RunOnThreadPool(Calculate);

                await UniTask.SwitchToMainThread();

                // UI 갱신 가능성이 있는 스트림은 메인 스레드에서 업데이트.
                point.Value = result.point;
                pointLimit.Value = result.pointLimit;
                lastUpdate.Value = result.lastUpdate;

                await UniTask.SwitchToThreadPool();
            }
        }
        
        private Calculation Calculate()
        {
            var now = NetworkService.CurrentTime();
            int limit = pointLimit.Value;
            var calculation = new Calculation
            {
                pointLimit = limit,
                lastUpdate = now
            };

            if (point.Value >= limit)
            {
                calculation.point = point.Value;
            }
            else
            {
                var minutes = (now - lastUpdate.Value).Minutes;
                int amount = minutes / ApAddIntervalMinute * ApAddPerOnce;
                int total = point.Value + amount;
                calculation.point = total < limit ? total : limit;
            }

            return calculation;
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

    public class UserRepository : IDisposable
    {
        public readonly string nickname;

        public readonly ReactiveProperty<GameEntrance> entrance;

        public int StageFocus
        {
            get => PlayerPrefs.GetInt(StageFocusKey, gameRecord.lastClearedStage.Value);
            private set => PlayerPrefs.SetInt(StageFocusKey, value);
        }

        public readonly Act act;
        public readonly Currency currency;
        public readonly Inventory inventory;
        public readonly Characters characters;
        public readonly Formation formation;
        public readonly GameRecord gameRecord;
        public readonly ShopRecord shopRecord;

        private const string StageFocusKey = "stage-focus";
        private const string DungeonFocusKey = "dungeon-focus-type_";

        public UserRepository(string nickname, UserDataDto dto)
        {
            this.nickname = nickname;

            act = new(dto.act);
            currency = new(dto.currency);
            inventory = new(dto.inventory);
            characters = new(dto.characters);
            formation = new(dto.formation);
            gameRecord = new(dto.gameRecord);
            shopRecord = new(dto.shopRecord);

            entrance = new(new GameEntrance
            {
                type = GameType.Stage,
                id = StageFocus
            });

            entrance
                .Subscribe(x =>
                {
                    if (x.type != GameType.Stage)
                        return;

                    StageFocus = x.id;
                });
        }

        public void Update(UserDataDto dto)
        {
            act.Update(dto.act);
            currency.Update(dto.currency);
            inventory.Update(dto.inventory);
            characters.Update(dto.characters);
            formation.Update(dto.formation);
            gameRecord.Update(dto.gameRecord);
            shopRecord.Update(dto.shopRecord);
        }

        public IEnumerable<EquipItem> EquipItems(IList<string> guids)
        {
            return inventory.items
                .OfType<EquipItem>()
                .Where(x => guids.Contains(x.Guid));
        }

        public void Dispose()
        {
            entrance?.Dispose();
            act?.Dispose();
            currency?.Dispose();
            inventory?.Dispose();
            characters?.Dispose();
            formation?.Dispose();
            gameRecord?.Dispose();
            shopRecord?.Dispose();
        }
    }
}