using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Data.Model;
using RGLabs.Lobby.Behaviours;
using RGLabs.Unit.Skill.Global;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RGLabs.InGame.UI
{
    public class UIGlobalSkill : UIListAdapter<UIGlobalSkillSlot, CastleSkillParameter>, IBeginDragHandler,
        IDragHandler, IEndDragHandler
    {
        [SerializeField] private Color _rangeColor;
        [SerializeField] private PolygonDrawer _rangeDrawer;

        private UIGlobalSkillSlot _selected;

        public override async UniTask Init(IEnumerable<CastleSkillParameter> collection)
        {
            _rangeDrawer.Init();
            _rangeDrawer.Color = _rangeColor;
            
            var parameters = collection as CastleSkillParameter[] ?? collection.ToArray();

            await base.Init(parameters);
            
            int length = parameters.Length;
            var castle = Storage.inGameRepository.castle.Value;
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
            _rangeDrawer.gameObject.SetActive(true);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_selected == null)
                return;

            var worldPos = eventData.position.ScreenToWorld();
            _rangeDrawer.transform.position = worldPos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            if (_selected == null)
                return;
            
            _selected.skill.Execute(eventData.position.ScreenToWorld());
            _selected = null;
            
            _rangeDrawer.gameObject.SetActive(false);
        }
    }
}