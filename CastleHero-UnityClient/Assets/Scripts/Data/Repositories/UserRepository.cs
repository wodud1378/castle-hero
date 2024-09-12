using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Data.Model;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UniRx;
using Unity.Collections;
using UnityEngine;

namespace RGLabs.Data.Repositories
{
    public struct GameEntrance
    {
        public GameType type;
        public int id;
    }

    public class UserRepository : IDisposable
    {
        public readonly string nickname;

        public readonly ReactiveProperty<GameEntrance> entrance;

        public int StageFocus
        {
            get => PlayerPrefs.GetInt(StageFocusKey, 1);
            //get => PlayerPrefs.GetInt(StageFocusKey, Mathf.Max(1, gameRecord.lastClearedStage.Value));
            set => PlayerPrefs.SetInt(StageFocusKey, value);
        }

        public readonly Stamina stamina;
        public readonly Currency currency;
        public readonly Inventory inventory;
        public readonly Characters characters;
        public readonly Formation formation;
        public readonly GameRecord gameRecord;
        public readonly ShopRecord shopRecord;

        private const string StageFocusKey = "stage-focus";

        public UserRepository(string nickname, UserDataDto dto)
        {
            this.nickname = nickname;

            stamina = new(dto.stamina);
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
            if (dto.stamina != null)
                stamina.Update(dto.stamina);

            if (dto.currency != null)
                currency.Update(dto.currency);

            if (dto.inventory != null)
                inventory.Update(dto.inventory);

            if (dto.characters != null)
                characters.Update(dto.characters);

            if (dto.formation != null)
                formation.Update(dto.formation);

            if (dto.gameRecord != null)
                gameRecord.Update(dto.gameRecord);

            if (dto.shopRecord != null)
                shopRecord.Update(dto.shopRecord);
        }

        public IEnumerable<EquipItem> EquipItems(IList<string> guids)
        {
            return inventory.items
                .OfType<EquipItem>()
                .Where(x => guids.Contains(x.Guid));
        }

        public UnitInfo UnitForLevelUp()
        {
            var units = UnitsInField();

            int maxLv = Storage.db.levels.MaxLv;
            return units.FirstOrDefault(x => x.lv < maxLv);
        }

        public UnitInfo UnitForUpgrade()
        {
            var units = UnitsInField();

            int maxRate = Storage.db.rates.MaxRate;
            return units.FirstOrDefault(x => x.rate < maxRate);
        }

        public UnitInfo UnitForUpgradeEquipments(out bool requireOtherEquipments)
        {
            requireOtherEquipments = false;

            var units = UnitsInField();

            // 아이템을 장착하지 않은 유닛이 존재할 때, 해당 유닛들 중 첫번째 유닛 반환.
            var notEquip = units.Find(unit => unit.equipments is not { Count: not 0 });
            if (notEquip != null)
                return notEquip;

            // 아이템 맵핑 후 캐싱.
            var unitMap = new Dictionary<UnitInfo, List<EquipItem>>();
            var dataMap = new Dictionary<EquipItem, EquipmentOption>();
            List<EquipItem> GetEquipItems(UnitInfo unit)
            {
                if (!unitMap.TryGetValue(unit, out var items))
                {
                    items = EquipItems(unit.equipments).ToList();
                    unitMap.Add(unit, items);
                }

                return items;
            }

            EquipmentOption GetOption(EquipItem item)
            {
                if (!dataMap.TryGetValue(item, out var option))
                {
                    option = Storage.db.items.TryFind(item.ItemId, out var e)
                        ? e.optionEquip
                        : default;
                    
                    dataMap.Add(item, option);
                }

                return option;
            }
            
            // 업그레이드 필요한 유닛 확인.
            int maxCount = (int)EquipmentSlot.Count;
            var requireUpgrade = units
                .Where(unit =>
                {
                    // 장비 슬롯이 비어있는 경우.
                    if (unit.equipments.Count < maxCount)
                        return true;

                    // 모든 장비가 레전드 등급 미만일 경우 True.
                    return GetEquipItems(unit)
                        .TrueForAll(x => GetOption(x).grade < EquipmentGrade.Legend);
                })
                .ToList();

            // 업그레이드 필요한 유닛이 없는 경우 null. (모든 유닛이 모든 슬롯에 레전드 등급 장착)
            if (requireUpgrade.Count == 0)
                return null;

            // 미사용중인 장비를 슬롯별로 가장 높은 등급 1개씩 필터링.
            var others = inventory.items
                .OfType<EquipItem>()
                .Where(item => item.character == 0)
                .GroupBy(item => item.slot)
                .Select(group 
                    => group.OrderByDescending(x => GetOption(x).grade).First())
                .ToList();

            int totalSum = 0;
            // 미사용중인 장비 장착시 올라가는 등급을 합산 및 해당 값에 따라 내림차순으로 정렬.
            var sorted = requireUpgrade
                .OrderByDescending(unit =>
                {
                    int sum = 0;
                    var equipments = GetEquipItems(unit);
                    foreach (var other in others)
                    {
                        var exist = equipments.Find(x => x.slot == other.slot);
                        int otherScore = (int)GetOption(other).grade;
                        int existScore = exist != null
                            ? (int)GetOption(exist).grade
                            : -1;
                        
                        // 비교 대상의 등급이 더 높은 경우에만 합산.
                        if (existScore < otherScore)
                        {
                            sum += otherScore - existScore;
                        }
                    }

                    totalSum += sum;
                    return sum;
                })
                .ToList();


            requireOtherEquipments = totalSum == 0;

            // 업그레이드 가능한 등급이 가장 높은 유닛을 반환.
            return sorted.First();
        }

        public List<UnitInfo> UnitsInField()
        {
            return characters.units
                .Where(unit => formation.fieldUnits.FirstOrDefault(x => x.id == unit.id) != default)
                .OrderBy(x => x.id)
                .ToList();
        }

        public void Dispose()
        {
            entrance?.Dispose();
            stamina?.Dispose();
            currency?.Dispose();
            inventory?.Dispose();
            characters?.Dispose();
            formation?.Dispose();
            gameRecord?.Dispose();
            shopRecord?.Dispose();
        }
    }
}