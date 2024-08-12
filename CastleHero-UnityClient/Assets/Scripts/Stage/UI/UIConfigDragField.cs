using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using RGLabs.Common;
using RGLabs.Common.Behaviours;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Lobby.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;

namespace RGLabs.Stage.UI
{
    public class UIConfigDragField : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerClickHandler
    {
        [SerializeField] private FormationField _formation;
        [SerializeField] private UIConfigCharacterList _characterList;
        [SerializeField] private PolygonDrawer _validationCircle;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        private readonly ReactiveProperty<UnitBehaviour> _unit = new();
        private CancellationTokenSource _ctSource;
        private int _originLayer;

        private bool _onDrag;


        private void Awake()
        {
            _validationCircle.Init();

            _unit
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
            _unit.Value = target;
            if (_unit.Value == null)
                return;

            _originLayer = target.gameObject.GetLayer();
            target.gameObject.ToPreviewLayer();
            target.Collider.isTrigger = true;
            target.Core.onRest.Value = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_unit.Value != null)
                return;

            _onDrag = true;

            var position = eventData.position.ScreenToWorld();
            if (!_formation.InArea(position))
                return;

            _unit.Value = FindFromRay(position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_unit.Value == null)
                return;

            _unit.Value.transform.position = eventData.position.ScreenToWorld();
            _validationCircle.Color =
                _formation.IsValid(_unit.Value.Collider, _originLayer) ? _validColor : _invalidColor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _ctSource?.Cancel();

            var hold = _unit.Value;
            if (hold == null)
                return;

            if (!_formation.TryRegister(hold, _originLayer, true))
            {
                hold.DestroySelf();
                _unit.Value = null;
                return;
            }

            hold.Core.movement.Default = _unit.Value.position;
            hold.Collider.isTrigger = false;
            hold.gameObject.ToLayer(_originLayer);

            _unit.Value = null;

            _validationCircle.Color = _validColor;
        }

        private UnitBehaviour FindFromRay(Vector2 position)
        {
            var hit = Physics2D.Raycast(position, Vector2.zero);
            if (hit.collider == null)
                return null;

            var unit = hit.collider.GetComponent<UnitBehaviour>();
            if (unit == Storage.inGameRepository.castle.Value)
                return null;

            return unit;
        }

        private async UniTaskVoid Create(UICharacterSlot slot, Vector2 position)
        {
            var factory = Context.unitFactory;
            var info = slot.Info;
            UnitBehaviour unit;
            if (info.id == Constants.BarricadeId)
                unit = await factory.CreateBarricade(info, position);
            else
                unit = await factory.Create(info, position);

            await UniTask.NextFrame();

            if (_formation.TryRegister(unit, _originLayer, false))
            {
                unit.Core.movement.Default = unit.position;
                return;
            }

            unit.DestroySelf();

            _validationCircle.Color = _invalidColor;

            await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

            _validationCircle.Color = _validColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_onDrag)
            {
                var position = eventData.position.ScreenToWorld();
                var slot = _characterList.selected.Value;
                if (slot != null)
                {
                    Create(slot, position).Forget();
                }
                else
                {
                    var selected = FindFromRay(position);
                    if (selected != null)
                        _formation.Remove(selected);
                }

                _characterList.selected.Value = null;
            }

            _onDrag = false;
        }
    }
}