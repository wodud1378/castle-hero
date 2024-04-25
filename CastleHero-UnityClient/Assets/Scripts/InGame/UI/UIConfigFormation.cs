using System;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.Data.DB;
using RGLabs.InGame.Data.Repositories;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIConfigFormation : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDisposable
    {
        public struct Result
        {
            public bool completed;
        }

        public enum Tab
        {
            Character,
            Barricade,
        }

        [SerializeField] private Formation _formation;
        [SerializeField] private Transform _slotParent;
        [SerializeField] private AssetLabelReference _slotPrefab;

        [SerializeField] private Button _complete;
        [SerializeField] private Button _cancel;

        [SerializeField] private UICharacterList _characterList;

        [SerializeField] private Color _vaildColor;
        [SerializeField] private Color _invalidColor;

        public ReactiveProperty<Tab> tab;

        private UnitBehaviour _hold;

        private UnitDB _db;
        private IUnitFactory _factory;

        private void Awake()
        {
            tab = new ReactiveProperty<Tab>(Tab.Character);

            _hold = null;

            _complete
                .OnClickAsObservable()
                .Subscribe(_ => Complete())
                .AddTo(this);

            _cancel
                .OnClickAsObservable()
                .Subscribe(_ => Cancel())
                .AddTo(this);

            _characterList.selected.Subscribe(OnSlotSelected);
        }

        public async void Open(UserRepository repository, UnitDB db, IUnitFactory factory)
        {
            _factory = factory;
            await _characterList.Init(repository, db);

            gameObject.SetActive(true);
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            var position = Input.GetTouch(0).position;
            var hit = Physics2D.Raycast(position, Vector2.zero);
            if (!hit.collider.TryGetComponent(out UnitBehaviour unit))
                return;

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

        private async void OnSlotSelected(UICharacterSlot slot)
        {
            var unit = await _factory.Create<UnitBehaviour>(slot.Entity, slot.transform.position);
            unit.canMove = false;
            unit.canAttack = false;
            unit.Collider.isTrigger = true;
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
                    unit.defaultDestination = unit.Position;
                    unit.gameObject.ToLayer("GroundUnit");
                }
            }

            _hold = null;
        }

        private void Complete()
        {
            var message = new Result { completed = true };
            message.Publish();

            Close();
        }

        private void Cancel()
        {
            var message = new Result { completed = false };
            message.Publish();

            Close();
        }

        private void Close()
        {
            gameObject.SetActive(false);
        }

        public void Dispose()
        {
            tab?.Dispose();
        }
    }
}