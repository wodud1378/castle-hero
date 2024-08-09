using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Network.Shared;

namespace RGLabs.InGame.UI
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