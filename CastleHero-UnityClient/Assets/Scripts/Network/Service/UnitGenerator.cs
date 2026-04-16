using System.Collections.Generic;
using CastleHero.Data;
using CastleHero.Network.Shared;
using CastleHero.Utility;

using CastleHero.Common.Pattern;
using CastleHero.Data.DB;
namespace CastleHero.Network.Service
{
    public class UnitGenerator
    {
        public void AddUnits(List<int> newUnitIds, List<UnitInfo> units, List<IItem> items, bool mergeSameSoulItem,
            out int newUnitStartIndex, out bool itemAdded)
        {
            units ??= new List<UnitInfo>();
            items ??= new List<IItem>();

            newUnitStartIndex = -1;
            itemAdded = false;

            foreach (var id in newUnitIds)
            {
                if (units.FindIndex(x => x.id == id) == -1)
                {
                    units.Add(NewUnit(id));
                    newUnitStartIndex = units.Count - 1;
                }
                else
                {
                    if (!ServiceLocator.Get<IDBProvider>().Units.TryFind(id, out var entity))
                        continue;

                    var item = new Item { ItemId = entity.soulItemId, Quantity = 10 };
                    if (mergeSameSoulItem)
                        items.Join(item);
                    else
                        items.Add(item);

                    itemAdded = true;
                }
            }
        }
        
        public UnitInfo NewUnit(int id) => NewUnit(id, 1, 0);

        private UnitInfo NewUnit(int id, int lv, int rate)
        {
            return new UnitInfo()
            {
                id = id,
                lv = lv,
                rate = rate,
                equipments = new List<string>(),
            };
        }
    }
}