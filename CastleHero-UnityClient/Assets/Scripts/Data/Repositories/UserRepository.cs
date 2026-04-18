using System;
using System.Collections.Generic;
using System.Linq;
using CastleHero.Data.Model;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UniRx;
using Unity.Collections;
using UnityEngine;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Data.Repositories
{
    public struct GameEntrance
    {
        public GameType type;
        public int id;
    }

    public class UserRepository : IUserRepository
    {
        public string Nickname { get; }

        public ReactiveProperty<GameEntrance> Entrance { get; }

        public int StageFocus
        {
            get => PlayerPrefs.GetInt(StageFocusKey, 1);
            set => PlayerPrefs.SetInt(StageFocusKey, value);
        }

        public Stamina Stamina { get; }
        public Currency Currency { get; }
        public Inventory Inventory { get; }
        public Characters Characters { get; }
        public Formation Formation { get; }
        public GameRecord GameRecord { get; }
        public ShopRecord ShopRecord { get; }

        private const string StageFocusKey = "stage-focus";
        private readonly CompositeDisposable _disposables = new();
        private readonly IDBProvider _db;

        public UserRepository(string nickname, UserDataDto dto)
        {
            _db = ServiceLocator.Instance.Get<IDBProvider>();
            Nickname = nickname;

            Stamina = new(dto.stamina);
            Currency = new(dto.currency);
            Inventory = new(dto.inventory);
            Characters = new(dto.characters);
            Formation = new(dto.formation);
            GameRecord = new(dto.gameRecord);
            ShopRecord = new(dto.shopRecord);

            Entrance = new(new GameEntrance
            {
                type = GameType.Stage,
                id = StageFocus
            });

            Entrance
                .Subscribe(x =>
                {
                    if (x.type != GameType.Stage)
                        return;

                    StageFocus = x.id;
                })
                .AddTo(_disposables);
        }

        public void Update(UserDataDto dto)
        {
            if (dto.stamina != null)
                Stamina.Update(dto.stamina);

            if (dto.currency != null)
                Currency.Update(dto.currency);

            if (dto.inventory != null)
                Inventory.Update(dto.inventory);

            if (dto.characters != null)
                Characters.Update(dto.characters);

            if (dto.formation != null)
                Formation.Update(dto.formation);

            if (dto.gameRecord != null)
                GameRecord.Update(dto.gameRecord);

            if (dto.shopRecord != null)
                ShopRecord.Update(dto.shopRecord);
        }

        public IEnumerable<EquipItem> EquipItems(IList<string> guids)
        {
            return Inventory.Items
                .OfType<EquipItem>()
                .Where(x => guids.Contains(x.Guid));
        }

        public UnitInfo UnitForLevelUp()
        {
            var units = UnitsInField();

            int maxLv = _db.Levels.MaxLv;
            return units.FirstOrDefault(x => x.lv < maxLv);
        }

        public UnitInfo UnitForUpgrade()
        {
            var units = UnitsInField();

            int maxRate = _db.Rates.MaxRate;
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
                    option = _db.Items.TryFind(item.ItemId, out var e)
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
            var others = Inventory.Items
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
            return Characters.Units
                .Where(unit => Formation.FieldUnits.FirstOrDefault(x => x.id == unit.id) != default)
                .OrderBy(x => x.id)
                .ToList();
        }

        public void Dispose()
        {
            _disposables.Dispose();
            Entrance?.Dispose();
            Stamina?.Dispose();
            Currency?.Dispose();
            Inventory?.Dispose();
            Characters?.Dispose();
            Formation?.Dispose();
            GameRecord?.Dispose();
            ShopRecord?.Dispose();
        }
    }
}