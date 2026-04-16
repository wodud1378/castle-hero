using CastleHero.View.Common.UI;
using CastleHero.GamePlay.Unit.Skill.Global;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.InGame.UI
{
    public class UIGlobalSkillSlot : UISlot
    {
        [FormerlySerializedAs("_coolTime")]
        [SerializeField] private Image coolTime;

        public GlobalSkill skill;

        private void Update()
        {
            if (skill == null)
                return;

            var timer = skill.coolTime.timer;
            coolTime.fillAmount = timer.leftTime / timer.time;
        }
    }
}
