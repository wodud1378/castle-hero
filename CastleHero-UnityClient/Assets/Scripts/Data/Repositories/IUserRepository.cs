using System;
using System.Collections.Generic;
using CastleHero.Network.Shared;
using UniRx;
using CastleHero.Data.Model;

namespace CastleHero.Data.Repositories
{
    public interface IUserRepository : IDisposable
    {
        string Nickname { get; }

        ReactiveProperty<GameEntrance> Entrance { get; }
        int StageFocus { get; set; }

        Stamina Stamina { get; }
        Currency Currency { get; }
        Inventory Inventory { get; }
        Characters Characters { get; }
        Formation Formation { get; }
        GameRecord GameRecord { get; }
        ShopRecord ShopRecord { get; }

        void Update(UserDataDto dto);

        IEnumerable<EquipItem> EquipItems(IList<string> guids);
        UnitInfo UnitForLevelUp();
        UnitInfo UnitForUpgrade();
        UnitInfo UnitForUpgradeEquipments(out bool requireOtherEquipments);
        List<UnitInfo> UnitsInField();
    }
}
