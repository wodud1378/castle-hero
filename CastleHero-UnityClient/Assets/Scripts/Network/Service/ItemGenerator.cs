using System;
using System.Collections.Generic;
using CastleHero.Common;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using Random = UnityEngine.Random;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Network.Service
{
    public class ItemGenerator
    {
        private readonly Dictionary<int, List<int>> _itemIdFilterCache = new();

        // 지연 조회: ItemGenerator 가 static readonly 로 타입 초기화 시 생성되므로 IDBProvider 가
        // 아직 등록되지 않은 상태. 각 메서드 진입 시 캐시 후 사용.
        private IDBProvider _dbCache;
        private IDBProvider Db => _dbCache ??= ServiceLocator.Get<IDBProvider>();

        public EquipItem NewEquipItem(int id)
        {
            if (!Db.Items.TryFind(id, out var entity))
                return null;

            return NewEquipItem(entity);
        }

        public EquipItem NewEquipItem(ItemEntity itemData)
        {
            var db = Db.EquipmentStats;
            var option = itemData.optionEquip;
            int mainStatId = option.mainStat;
            int statValueIndex = (int)option.grade;
            var main =
                mainStatId != -1
                    ? NewMainStat(mainStatId, statValueIndex)
                    : NewMainStat(
                        Random.Range(0, db.Length),
                        (int)option.grade
                    );

            int subStatCount = option.grade switch
            {
                EquipmentGrade.Legend => 8,
                EquipmentGrade.Epic => 6,
                EquipmentGrade.Rare => 4,
                EquipmentGrade.Common => 2,
                _ => 0,
            };

            var sub = new List<EquipItem.Stat>();
            for (int i = 0; i < subStatCount; ++i)
            {
                sub.Add(NewSubStat(Random.Range(0, db.Length), statValueIndex));
            }

            return new EquipItem
            {
                ItemId = itemData.Id,
                Guid = Guid.NewGuid().ToString(),
                main = main,
                sub = sub,
                slot = (int)option.slot,
                Quantity = 1,
                element = new EquipItem.Element { type = 0, lv = 0, }
            };
        }

        public void NewItems(int[] ids, int[] quantities, CurrencyDto currency, List<IItem> items)
        {
            currency ??= new CurrencyDto();
            items ??= new List<IItem>();

            int index = 0;
            int count = Math.Min(ids.Length, quantities.Length);
            while (index < count)
            {
                int id = ids[index];
                int quantity = quantities[index];

                Generate(id, quantity, currency, items);

                ++index;
            }
        }

        public (CurrencyDto currency, List<IItem> items) NewItems(int[] ids, int[] quantities)
        {
            var currency = new CurrencyDto();
            var items = new List<IItem>();

            int index = 0;
            int count = Math.Min(ids.Length, quantities.Length);
            while (index < count)
            {
                int id = ids[index];
                int quantity = quantities[index];

                Generate(id, quantity, currency, items);

                ++index;
            }

            return (currency, items);
        }

        public void NewItems(int id, int quantity, CurrencyDto currency, List<IItem> items)
        {
            currency ??= new CurrencyDto();
            items ??= new List<IItem>();

            Generate(id, quantity, currency, items);
        }

        public (CurrencyDto currency, List<IItem> items) NewItems(int id, int quantity)
        {
            var currency = new CurrencyDto();
            var items = new List<IItem>();

            Generate(id, quantity, currency, items);

            return (currency, items);
        }

        private void Generate(int id, int quantity, CurrencyDto currency, List<IItem> items)
        {
            switch (id)
            {
                case Constants.PaidDiaId:
                    currency.paidDia += quantity;
                    break;
                case Constants.FreeDiaId:
                    currency.freeDia += quantity;
                    break;
                case Constants.GoldId:
                    currency.gold += quantity;
                    break;
                default:
                    var related = id.GetRelatedItemIds();
                    if (related.Count == 1)
                    {
                        var item = CreateItem(related[0], quantity);
                        if (item != null)
                            items.Join(item);
                    }
                    else
                    {
                        for (int i = 0; i < quantity; ++i)
                        {
                            int rand = Random.Range(0, related.Count);
                            id = related[rand];

                            var item = CreateItem(id, 1);
                            if (id.IsEquipItem())
                                items.Add(item);
                            else
                                items.Join(item);
                        }
                    }

                    break;
            }
        }

        private IItem CreateItem(int id, int quantity)
        {
            if (!ServiceLocator.Get<IDBProvider>().Items.TryFind(id, out var data))
            {
#if UNITY_EDITOR
                Debug.LogError($"[{id}] 해당하는 아이템이 존재하지 않습니다.");
#endif
                return null;
            }

            var type = data.type;
            return type switch
            {
                ItemType.Equipment => NewEquipItem(data),
                _ => new Item { ItemId = data.Id, Quantity = quantity }
            };
        }

        private EquipItem.Stat NewMainStat(int status, int index)
        {
            if (!ServiceLocator.Get<IDBProvider>().EquipmentStats.TryFind(status, out var entity) &&
                !index.IsValidIndex(entity.mainMin, entity.mainMax))
                return default;

            return new EquipItem.Stat
            {
                type = status,
                value = RandomValue(entity.mainMin[index], entity.mainMax[index])
            };
        }

        private EquipItem.Stat NewSubStat(int status, int index)
        {
            if (!ServiceLocator.Get<IDBProvider>().EquipmentStats.TryFind(status, out var entity) &&
                !index.IsValidIndex(entity.subMin, entity.subMax))
                return default;

            return new EquipItem.Stat
            {
                type = status,
                value = RandomValue(entity.subMin[index], entity.subMax[index])
            };
        }

        private float RandomValue(float min, float max)
        {
            var randomValue = Random.Range(min, max);
            return Mathf.Round(randomValue * 100000f) / 100000f;
        }
    }
}