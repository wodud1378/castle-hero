using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Lobby.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RGLabs.Lobby.UI
{
    public class UIConfigDragField : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [SerializeField] private Formation _formation;
        [SerializeField] private PolygonDrawer _validationCircle;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        public ReactiveProperty<UnitBehaviour> unit = new();

        private CancellationTokenSource _ctSource;
        private int _originLayer;

        private void Awake()
        {
            _validationCircle.Init();
            
            unit
                .Subscribe(OnTargetChanged)
                .AddTo(this);
        }

        private void OnEnable()
        {
            _validationCircle.gameObject.SetActive(true);
            _validationCircle.Color = _validColor;
        }

        private void OnDisable() => _validationCircle.gameObject.SetActive(false);

        private void OnTargetChanged(UnitBehaviour target)
        {
            unit.Value = target;
            if (unit.Value == null)
                return;

            _originLayer = target.gameObject.layer;
            target.gameObject.ToPreviewLayer();
            target.Core.inBattle = false;
            target.Collider.isTrigger = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (unit.Value != null)
                return;
            
            PressTask(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (unit.Value == null)
                return;

            unit.Value.transform.position = eventData.position.ScreenToWorld();
            _validationCircle.Color =
                _formation.IsValid(unit.Value.Collider, _originLayer) ? _validColor : _invalidColor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _ctSource?.Cancel();
            
            if (unit.Value == null)
                return;

            if (!_formation.TryRegister(unit.Value, _originLayer))
            {
                unit.Value.DestroySelf();
                return;
            }

            unit.Value.Core.movement.Default = unit.Value.position;
            unit.Value.Collider.isTrigger = false;
            unit.Value.gameObject.layer = _originLayer;

            unit.Value = null;
            
            _validationCircle.Color = _validColor;
        }

        private async void PressTask(PointerEventData eventData)
        {
            _ctSource = new();

            await UniTask
                .Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: _ctSource.Token)
                .SuppressCancellationThrow();

            var position = eventData.position.ScreenToWorld();
            bool isCancel = _ctSource.IsCancellationRequested;
            _ctSource = null;
            
            if (isCancel)
            {
                var found = FindFromRay(position);
                if(found != null)
                    _formation.Remove(found);
                
                return;
            }

            if (!_formation.InArea(position))
                return;

            unit.Value = FindFromRay(position);
        }

        private UnitBehaviour FindFromRay(Vector2 position)
        {
            var hit = Physics2D.Raycast(position, Vector2.zero);
            if (hit.collider == null)
                return null;

            return hit.collider.GetComponent<UnitBehaviour>();
        }
    }
}