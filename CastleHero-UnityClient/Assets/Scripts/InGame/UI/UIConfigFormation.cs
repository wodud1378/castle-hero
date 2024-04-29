using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.InGame.Behaviours;
using RGLabs.InGame.Behaviours.Unit;
using RGLabs.InGame.System.UnitFactory;
using RGLabs.InGame.Utility;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace RGLabs.InGame.UI
{
    public class UIConfigFormation : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler, IDisposable
    {
        [SerializeField] private Graphic _rayTarget;
        [SerializeField] private Rect _areaBounds;

        [SerializeField] private Formation _formation;

        [SerializeField] private UICharacterList _characterList;

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
            var unit = await _factory.Create<UnitBehaviour>(slot.Entity, slot.transform.position);
            unit.canMove = false;
            unit.canAttack = false;
            unit.Collider.isTrigger = true;
            unit.transform.localScale = Vector3.one * 2f;

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
                _hold.Collider.isTrigger = false;
                _hold.defaultDestination = _hold.position;
                _hold.gameObject.layer = _originLayer;
                _hold.transform.localScale = Vector3.one;
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
            if (_areaBounds.Contains(position))
                return FindFromRay(position);
            
            var slot = _characterList.GetSlot(position);
            if (slot == null)
                return null;
            
            return await CreateUnitFromSlot(slot);
        }
        
        public void Dispose()
        {
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawWireCube(_areaBounds.position, _areaBounds.size);
        }

        private void OnEnable()
        {
            _camera.DOOrthoSize(10f, 0.25f);

            _rayTarget.enabled = true;
        }

        private void OnDisable()
        {
            _camera.DOOrthoSize(15f, 0.25f);

            _rayTarget.enabled = false;
        }
    }
}