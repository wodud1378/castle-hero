using System;
using System.Collections.Generic;
using System.Linq;
using RGLabs.Network.Shared;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using Unity.VisualScripting;

namespace RGLabs.Data.Repositories
{
    public class Profile
    {
        public readonly ReactiveProperty<int> iconId;
        public readonly ReactiveProperty<int> stage;
        public readonly ReactiveProperty<int> focusedStage;
        public readonly ReactiveProperty<int> castleLv;

        public Profile(ProfileDto dto)
        {
            iconId = new(dto.iconId);
            stage = new(dto.stage);
            focusedStage = new(dto.focusedStage);
            castleLv = new(dto.castleLv);
        }

        public void Update(ProfileDto dto)
        {
            iconId.Value = dto.iconId;
            stage.Value = dto.stage;
            focusedStage.Value = dto.focusedStage;
            castleLv.Value = dto.castleLv;
        }
    }

    public class Act
    {
        public readonly ReactiveProperty<int> point;
        public readonly ReactiveProperty<int> pointLimit;
        public readonly ReactiveProperty<DateTime> lastUpdate;

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
    }

    public class Currency
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
    }

    public class Inventory
    {
        public readonly ReactiveCollection<IItem> items;

        public Inventory(InventoryDto dto) => items = new(dto.items);

        public void Update(InventoryDto dto)
        {
            items.Clear();
            items.AddRange(dto.items);
        }

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
    }

    public class Characters
    {
        public readonly ReactiveCollection<UnitInfo> units;

        public Characters(CharactersDto dto) => units = new(dto.units);

        public void Update(CharactersDto dto)
        {
            units.Clear();
            units.AddRange(dto.units);
        }

        public void Add(UnitInfo unit)
        {
            if (units.FirstOrDefault(x => x.id == unit.id) != null)
                return;

            units.Add(unit);
        }

        public void Update(UnitInfo unit) => units.Update(unit, x => x.id == unit.id);
    }

    public class Formation
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

        public void Update(FormationDto dto)
        {
            fieldUnits.Clear();
            fieldUnits.AddRange(dto.fieldUnits);
        }
    }

    public class UserRepository
    {
        public readonly string nickname;

        public readonly Profile profile;
        public readonly Act act;
        public readonly Currency currency;
        public readonly Inventory inventory;
        public readonly Characters characters;
        public readonly Formation formation;

        public UserRepository(string nickname, UserDataDto dto)
        {
            this.nickname = nickname;

            profile = new(dto.profile);
            act = new(dto.act);
            currency = new(dto.currency);
            inventory = new(dto.inventoryDto);
            characters = new(dto.characters);
            formation = new(dto.formation);
        }
        
        public IEnumerable<EquipItem> EquipItems(IList<string> guids)
        {
            return inventory.items
                .OfType<EquipItem>()
                .Where(x => guids.Contains(x.Guid));
        }
    }
}