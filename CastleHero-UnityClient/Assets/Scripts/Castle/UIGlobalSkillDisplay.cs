using Cysharp.Threading.Tasks;
using RGLabs.Common.UI;
using RGLabs.Data.Model;

namespace RGLabs.Castle
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