using System;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIConfigFormation : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDisposable
    {
        public enum Tab
        {
            Character,
            Barricade,
        }

        [SerializeField] private Formation _formation;
        [SerializeField] private Button _openList;

        [SerializeField] private UICharacterList _characterList;

        public ReactiveProperty<Tab> tab;
        public Button back;
        
        private int _originLayer;
        private UnitBehaviour _hold;

        private IUnitFactory _factory;
        
        public void Init()
        {
            tab = new ReactiveProperty<Tab>(Tab.Character);

            _hold = null;
            
            _characterList.selected.Subscribe(OnSlotSelected);
        }

        public async void Set(UserRepository repository, UnitDB db, IUnitFactory factory)
        {
            _factory = factory;
            
            await _characterList.Init(repository, db);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var unit = FindFromRay(eventData.position);
            OnUnitSelected(unit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_hold == null)
                return;

            _hold.transform.position = eventData.position;
            bool isValid = _formation.IsValid(_hold.Collider);
        }
        
        public void OnEndDrag(PointerEventData _) => OnUnitSelected(null);

        private UnitBehaviour FindFromRay(Vector2 position)
        {
            var hit = Physics2D.Raycast(position, Vector2.zero);
            if (!hit.collider.TryGetComponent(out UnitBehaviour unit))
                return null;

            return unit;
        }
        
        private async void OnSlotSelected(UICharacterSlot slot)
        {
            if (slot == null)
                return;
            
            var unit = await _factory.Create<UnitBehaviour>(slot.Entity, slot.transform.position);
            unit.canMove = false;
            unit.canAttack = false;
            unit.Collider.isTrigger = true;

            _originLayer = unit.gameObject.layer;
            unit.gameObject.ToUILayer();

            OnUnitSelected(unit);
        }

        private void OnUnitSelected(UnitBehaviour unit)
        {
            if (_hold == null && unit == null)
                return;

            if (unit == null)
            {
                if (!_formation.TryRegister(unit))
                {
                    unit.DestroySelf();
                }
                else
                {
                    unit.Collider.isTrigger = false;
                    unit.defaultDestination = unit.position;
                    unit.gameObject.layer = _originLayer;
                }
            }

            _hold = null;
        }
        
        public void Dispose()
        {
            tab?.Dispose();
            
            _characterList.Dispose();
        }
    }
}