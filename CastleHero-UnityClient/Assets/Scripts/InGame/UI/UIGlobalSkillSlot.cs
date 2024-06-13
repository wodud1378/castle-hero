using RGLabs.Common.UI;
using RGLabs.Unit.Skill.Global;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIGlobalSkillSlot : UISlot
    {
        [SerializeField] private Image _coolTime;

        public GlobalSkill skill;

        private void Update()
        {
            if (skill == null)
                return;

            var timer = skill.coolTime.timer;
            _coolTime.fillAmount = timer.leftTime / timer.time;
        }
    }
}