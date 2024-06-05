using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.Model;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UIConfigCharacterList : UICharacterList, IPointerDownHandler, IDragHandler, IPointerUpHandler
    {
        private static readonly int UnFold = Animator.StringToHash("UnFold");
        private static readonly int Fold = Animator.StringToHash("Fold");

        [SerializeField] private UIConfigDragField _dragField;
        [SerializeField] private Animator _animator;
        [SerializeField] private Formation _formation;
        [SerializeField] private Button _close;
        [SerializeField] private Button _reset;
        [SerializeField] private Button _auto;
        [SerializeField] private TMP_Text _placedUnit;
        [SerializeField] private float _dragThreshold = 0.3f;

        private Vector2 _startAt;
        private Vector2 _current;

        private float _holdTime;
        private bool _onHold;

        private IUnitFactory _factory;
        
        public bool IsOpen { get; private set; }

        private CancellationTokenSource _ctSource;
        private int _originLayer;

        private void Awake()
        {
            this.SubscribeButton(_close, Close);
            this.SubscribeButton(_reset, _formation.Clear);
            this.SubscribeButton(_auto, _formation.AutoPlacement);
        }

        public void Open()
        {
            IsOpen = true;

            _dragField.gameObject.SetActive(true);
            
            _factory = Storage.unitFactory;
            
            Entrance();
        }

        private void Entrance()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOMoveY(-3.75f, 0.25f);
                cam.DOOrthoSize(12.5f, 0.25f);  
            }
            
            _animator.SetTrigger(UnFold);
        }

        public void Close()
        {
            IsOpen = false;

            _dragField.gameObject.SetActive(false);

            Exit();
        }

        private void Exit()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOMoveY(0, 0.25f);
                cam.DOOrthoSize(15f, 0.25f);
            }
            
            _animator.SetTrigger(Fold);

            TaskHelper.OnAnimationEnd(_animator, Fold, Dispose)
                .Forget();
        }
        
        public void OnPointerDown(PointerEventData eventData)
        {
            _startAt = eventData.position;

            PressTask(eventData);
        }
        
        public void OnDrag(PointerEventData eventData)
        {
            if (Vector2.Distance(_startAt, eventData.position) > _dragThreshold)
            {
                _ctSource?.Cancel();
            }
        }

        public void OnPointerUp(PointerEventData eventData) => _ctSource?.Cancel();

        protected override UniTask SetItem(UICharacterSlot item, UnitInfo data, CancellationToken ct)
        {
            item.ReceiveRay = false;

            return base.SetItem(item, data, ct);
        }

        private async void PressTask(PointerEventData eventData)
        {
            _ctSource = new();

            await UniTask
                .Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: _ctSource.Token)
                .SuppressCancellationThrow();

            if (_ctSource.Token.IsCancellationRequested)
                return;

            var unit = await CreateFromPosition(eventData);
            if (unit != null)
            {
                _dragField.unit.Value = unit;

                var obj = _dragField.gameObject;
                eventData.pointerDrag = obj;
                ExecuteEvents.Execute(obj, eventData, ExecuteEvents.dragHandler);
            }
            else
                _dragField = null;
        }
        
        private async UniTask<UnitBehaviour> CreateFromPosition(PointerEventData eventData)
        {
            var slot = GetItem(eventData);
            if (slot == null)
                return null;
            
            return await _factory.Create(slot.Info, slot.transform.position);
        }
    }
}