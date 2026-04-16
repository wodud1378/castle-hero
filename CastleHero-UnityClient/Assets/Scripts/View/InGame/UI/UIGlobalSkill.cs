using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.Data.Model;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.GamePlay.Unit.Skill.Global;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;
using CastleHero.GamePlay.InGame;

namespace CastleHero.View.InGame.UI
{
    public class UIGlobalSkill : UIListAdapter<UIGlobalSkillSlot, CastleSkillParameter>, IBeginDragHandler,
        IDragHandler, IEndDragHandler
    {
        [FormerlySerializedAs("_rangeColor")]
        [SerializeField] private Color rangeColor;
        [FormerlySerializedAs("_rangeDrawer")]
        [SerializeField] private PolygonDrawer rangeDrawer;

        private UIGlobalSkillSlot _selected;

        public override async UniTask Init(IEnumerable<CastleSkillParameter> collection)
        {
            rangeDrawer.Init();
            rangeDrawer.Color = rangeColor;

            var parameters = collection as CastleSkillParameter[] ?? collection.ToArray();
            await base.Init(parameters);

            int length = parameters.Length;
            var castle = InGameSession.Current.Castle.Value as UnitBehaviour;
            for (int i = 0; i < length; ++i)
            {
                items[i].skill = new GlobalSkill(castle, parameters[i]);
            }
        }

        protected override UniTask SetItem(UIGlobalSkillSlot slot,CastleSkillParameter data) => slot.Init(data.icon);

        public void OnBeginDrag(PointerEventData eventData)
        {
            var slot = GetItem(eventData);
            if (slot == null)
                return;

            var skill = slot.skill;
            if (!skill.coolTime.IsReady)
                return;

            _selected = slot;
            rangeDrawer.UpdateSize(skill.radius);
            rangeDrawer.gameObject.SetActive(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_selected == null)
                return;

            var worldPos = eventData.position.ScreenToWorld();
            rangeDrawer.transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_selected == null)
                return;

            _selected.skill.Execute(eventData.position.ScreenToWorld());
            _selected = null;

            rangeDrawer.gameObject.SetActive(false);
        }
    }
}
