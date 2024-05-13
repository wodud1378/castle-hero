using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.Behaviours;
using RGLabs.InGame.UI;
using RGLabs.Lobby.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Unit.Factory;
using RGLabs.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.Lobby.UI
{
    public class UIConfigFormation : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IDisposable
    {
        [SerializeField] private Graphic _rayTarget;
        [SerializeField] private Formation _formation;
        [SerializeField] private UICharacterList _characterList;
        [SerializeField] private PolygonDrawer _circleDrawer;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;
        
        private Camera _camera;
        private int _originLayer;
        private UnitBehaviour _hold;

        private IUnitFactory _factory;

        public void Init()
        {
            _camera = Camera.main;
            _factory = _formation.Factory;

            _hold = null;
        }
        
        private UnitBehaviour FindFromRay(Vector2 position)
        {
            var hit = Physics2D.Raycast(position, Vector2.zero);
            if (hit.collider == null)
                return null;
            
            return hit.collider.GetComponent<UnitBehaviour>();
        }

        private async UniTask<UnitBehaviour> CreateUnitFromSlot(UICharacterSlot slot)
        {
            var unit = await _factory.Create(slot.Entity, slot.transform.position);
            unit.CanMove = false;
            unit.CanAttack = false;
            unit.Collider.isTrigger = true;

            _originLayer = unit.gameObject.layer;
            unit.gameObject.ToUILayer();
            return unit;
        }

        private void HoldUnit(UnitBehaviour unit)
        {
            if (unit == null)
                return;
            
            _hold = unit;
        }

        private void ReleaseHeldUnit()
        {
            if (_hold == null)
                return;
            
            if (!_formation.TryRegister(_hold, _originLayer))
            {
                _hold.DestroySelf();
            }
            else
            {
                _hold.Core.defaultDestination = _hold.position;
                _hold.CanMove = true;
                _hold.Collider.isTrigger = false;
                _hold.gameObject.layer = _originLayer;
            }

            _hold = null;
        }
        
        public async void OnPointerDown(PointerEventData eventData)
        {
            if (!_characterList.IsOpen)
                return;

            var position = ScreenToWorld(eventData.position);
            var unit = await PressTask(position);
            if (unit == null)
                return;
            
            HoldUnit(unit);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_hold == null)
                return;
            
            _hold.transform.position = ScreenToWorld(eventData.position);
            bool isValid = _formation.IsValid(_hold.Collider, _originLayer);

            _circleDrawer.Color = isValid ? _validColor : _invalidColor;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_characterList.IsOpen)
            {
                _characterList.Open();
                return;
            }
            
            _ctk?.Cancel();
            
            ReleaseHeldUnit();

            _circleDrawer.Color = _validColor;
        }

        private Vector3 ScreenToWorld(Vector3 screenPoint)
        {
            var position = _camera.ScreenToWorldPoint(screenPoint);
            position.z = 0;

            return position;
        }

        private CancellationTokenSource _ctk;
        
        private async UniTask<UnitBehaviour> PressTask(Vector2 position)
        {
            _ctk = new CancellationTokenSource();
            
            await UniTask.Delay(TimeSpan.FromSeconds(0.2f), cancellationToken: _ctk.Token).SuppressCancellationThrow();

            if (!_ctk.Token.IsCancellationRequested) 
                return await GetUnitFromPosition(position);
            
            _formation.Remove(FindFromRay(position));
            return null;
        }

        private async UniTask<UnitBehaviour> GetUnitFromPosition(Vector2 position)
        {
            if (_formation.InArea(position))
                return FindFromRay(position);
            
            var slot = _characterList.GetSlot(position);
            if (slot == null)
                return null;
            
            return await CreateUnitFromSlot(slot);
        }
        
        public void Dispose()
        {
        }

        private void OnEnable()
        {
            _camera.transform.DOMoveY(-3.75f, 0.25f);
            _camera.DOOrthoSize(12.5f, 0.25f);

            _rayTarget.enabled = true;
            
            _circleDrawer.Init();
            _circleDrawer.gameObject.SetActive(true);
            _circleDrawer.Color = _validColor;

            Context.startButton.enabled = false;
        }

        private void OnDisable()
        {
            _camera.transform.DOMoveY(0, 0.25f);
            _camera.DOOrthoSize(15f, 0.25f);

            _rayTarget.enabled = false;
            
            _circleDrawer.gameObject.SetActive(false);
            
            Context.startButton.enabled = true;
        }
    }
}