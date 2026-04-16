using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI;
using CastleHero.Data.Model;

namespace CastleHero.View.Castle
{
    public class UIGlobalSkillDisplay : UIListAdapter<UISlot, CastleSkillParameter>
    {
        protected override UniTask SetItem(UISlot slot, CastleSkillParameter data)
        {
            slot.OnClick += (s)=> OnClickSlot(slot, data);
            return slot.Init(data.icon);
        }

        private void OnClickSlot(UISlot slot, CastleSkillParameter data)
        {
            // TODO 스킬 툴팁.   
        }
    }
}