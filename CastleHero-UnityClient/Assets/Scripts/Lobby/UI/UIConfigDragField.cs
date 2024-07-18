using System.Threading;
using RGLabs.Common;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Lobby.Behaviours;
using RGLabs.Unit.Behaviours;
using RGLabs.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace RGLabs.Lobby.UI
{
    public class UIConfigDragField : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerClickHandler
    {
        public enum UnitFrom
        {
            Slot,
            Field
        }
        
        [FormerlySerializedAs("_formation")] [SerializeField] private FormationField formationField;
        [SerializeField] private PolygonDrawer _validationCircle;
        [SerializeField] private Color _validColor;
        [SerializeField] private Color _invalidColor;

        private UnitFrom _unitFrom;
        private readonly ReactiveProperty<UnitBehaviour> _unit = new();
        private CancellationTokenSource _ctSource;
        private int _originLayer;

        private bool _onDrag;

        public async void Create(UICharacterSlot slot)
        {
            var factory = Storage.unitFactory;

            var info = slot.Info;
            UnitBehaviour unit;
            if(info.id == Constants.BarricadeId)
                unit = await factory.CreateBarricade(info, slot.transform.position);
            else
                unit = await factory.Create(info, slot.transform.position);

            SetUnit(UnitFrom.Slot, unit);
        }
        
        private void SetUnit(UnitFrom unitFrom, UnitBehaviour unit)
        {
            _unitFrom = unitFrom;
            _unit.Value = unit;
        }
        
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
            if (!formationField.InArea(position))
                return;

            SetUnit(UnitFrom.Field, FindFromRay(position));
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_unit.Value == null)
                return;

            _unit.Value.transform.position = eventData.position.ScreenToWorld();
            _validationCircle.Color =
                formationField.IsValid(_unit.Value.Collider, _originLayer) ? _validColor : _invalidColor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _ctSource?.Cancel();

            var hold = _unit.Value;
            if (hold == null)
                return;

            if (!formationField.TryRegister(hold, _originLayer, _unitFrom == UnitFrom.Field))
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

            return hit.collider.GetComponent<UnitBehaviour>();
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_onDrag)
            {
                var selected = FindFromRay(eventData.position.ScreenToWorld());
                if (selected != null)
                    formationField.Remove(selected);
            }

            _onDrag = false;
        }
    }
}