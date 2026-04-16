using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using CastleHero.Common;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Pattern;
using CastleHero.View.Common.UI;
using CastleHero.Data;
using CastleHero.View.Lobby.Behaviours;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Utility;
using UniRx;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.Prepare.UI
{
    public class UIConfigDragField : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler,
        IPointerClickHandler
    {
        [FormerlySerializedAs("_formation")]
        [SerializeField] private FormationField formation;
        [FormerlySerializedAs("_characterList")]
        [SerializeField] private UIConfigCharacterList characterList;
        [FormerlySerializedAs("_validationCircle")]
        [SerializeField] private PolygonDrawer validationCircle;
        [FormerlySerializedAs("_validColor")]
        [SerializeField] private Color validColor;
        [FormerlySerializedAs("_invalidColor")]
        [SerializeField] private Color invalidColor;

        private readonly ReactiveProperty<UnitBehaviour> _unit = new();
        private CancellationTokenSource _ctSource;
        private int _originLayer;

        private bool _onDrag;

        private void Awake()
        {
            validationCircle.Init();

            _unit
                .Subscribe(OnTargetChanged)
                .AddTo(this);
        }

        private void OnEnable()
        {
            validationCircle.gameObject.SetActive(true);
            validationCircle.Color = validColor;
        }

        private void OnDisable() => validationCircle.gameObject.SetActive(false);

        private void OnTargetChanged(UnitBehaviour target)
        {
            _unit.Value = target;
            if (_unit.Value == null)
                return;

            _originLayer = target.gameObject.GetLayer();
            target.gameObject.ToPreviewLayer();
            target.Collider.isTrigger = true;
            target.Core.OnRest.Value = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (_unit.Value != null)
                return;

            _onDrag = true;

            var position = eventData.position.ScreenToWorld();
            if (!formation.InArea(position))
                return;

            _unit.Value = FindFromRay(position);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (_unit.Value == null)
                return;

            _unit.Value.transform.position = eventData.position.ScreenToWorld();
            validationCircle.Color =
                formation.IsValid(_unit.Value.Collider, _originLayer) ? validColor : invalidColor;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            _ctSource?.Cancel();

            var hold = _unit.Value;
            if (hold == null)
                return;

            if (!formation.TryRegister(hold, _originLayer, true))
            {
                hold.DestroySelf();
                _unit.Value = null;
                return;
            }

            hold.Core.Movement.Default = _unit.Value.Position;
            hold.Collider.isTrigger = false;
            hold.gameObject.ToLayer(_originLayer);

            _unit.Value = null;

            validationCircle.Color = validColor;
        }

        private UnitBehaviour FindFromRay(Vector2 position)
        {
            var hit = Physics2D.Raycast(position, Vector2.zero);
            if (hit.collider == null)
                return null;

            var unit = hit.collider.GetComponent<UnitBehaviour>();
            if (unit == formation.Draft.Castle.Value as UnitBehaviour)
                return null;

            return unit;
        }

        private async UniTaskVoid Create(UICharacterSlot slot, Vector2 position)
        {
            var factory = ServiceLocator.Get<CastleHero.Data.Factory.IUnitFactory>();
            var info = slot.Info;
            var created = info.id == Constants.BarricadeId
                ? await factory.CreateBarricade(info, position)
                : await factory.Create(info, position);

            await UniTask.NextFrame();

            if (created is not UnitBehaviour unit)
            {
                created?.DestroySelf();
                return;
            }

            if (formation.TryRegister(unit, _originLayer, false))
            {
                unit.Core.Movement.Default = unit.Position;
                return;
            }

            unit.DestroySelf();

            validationCircle.Color = invalidColor;

            await UniTask.Delay(TimeSpan.FromSeconds(0.25f));

            validationCircle.Color = validColor;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (!_onDrag)
            {
                var position = eventData.position.ScreenToWorld();
                var slot = characterList.selected.Value;
                if (slot != null)
                {
                    Create(slot, position).Forget();
                }
                else
                {
                    var selected = FindFromRay(position);
                    if (selected != null)
                        formation.Remove(selected);
                }

                characterList.selected.Value = null;
            }

            _onDrag = false;
        }
    }
}
