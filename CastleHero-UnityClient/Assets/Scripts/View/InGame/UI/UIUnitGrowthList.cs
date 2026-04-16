using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Network.Shared;

namespace CastleHero.View.InGame.UI
{
    public class UIUnitGrowthList : UIListAdapter<UIUnitGrowthSlot, UnitTransition>
    {
        public void PlayDirection()
        {
            foreach (var item in items)
            {
                item.Show();
            }
        }
        
        protected override UniTask SetItem(UIUnitGrowthSlot slot, UnitTransition data) => slot.Init(data);
    }
}