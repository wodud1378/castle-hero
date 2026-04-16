using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Pattern;
using CastleHero.View.Common.UI;
using CastleHero.View.Common.UI.Popup;
using CastleHero.Data;
using CastleHero.View.Lobby.Behaviours;
using CastleHero.Network.Shared;
using CastleHero.GamePlay.Unit.Behaviours;
using CastleHero.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Serialization;
using CastleHero.Data.Model;

namespace CastleHero.View.Lobby.Prepare.UI
{
    public class UIConfigCharacterList : UICharacterList
    {
        private static readonly int UnFold = Animator.StringToHash("UnFold");
        private static readonly int Fold = Animator.StringToHash("Fold");

        [FormerlySerializedAs("_animator")]
        [SerializeField] private Animator animator;
        [FormerlySerializedAs("_formation")]
        [SerializeField] private FormationField formation;
        [FormerlySerializedAs("_close")]
        [SerializeField] private Button close;
        [FormerlySerializedAs("_reset")]
        [SerializeField] private Button reset;
        [FormerlySerializedAs("_auto")]
        [SerializeField] private Button auto;
        [FormerlySerializedAs("_confirm")]
        [SerializeField] private Button confirm;
        [FormerlySerializedAs("_placedUnit")]
        [SerializeField] private TMP_Text placedUnit;

        public UniTask ConfigTask => _completionSource.Task;

        private UniTaskCompletionSource _completionSource;

        public readonly BoolReactiveProperty isOpened = new(false);
        public readonly ReactiveProperty<UICharacterSlot> selected = new(null);

        private Vector2 _startAt;
        private Vector2 _current;

        private float _holdTime;
        private bool _onHold;

        private int _originLayer;

        private void Awake()
        {
            formation.Draft.Characters
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(_ => Init(ServiceLocator.Get<CastleHero.Data.Repositories.IUserRepository>().Characters.Units))
                .AddTo(this);

            formation.placed
                .CombineLatest(formation.capacity, (current, max) => (current, max))
                .ThrottleFrame(1)
                .Subscribe(x => placedUnit.text = $"{x.current}/{x.max}")
                .AddTo(this);

            selected
                .Subscribe(x =>
                {
                    foreach (var item in items)
                    {
                        item.state.Value = item == x
                            ? UIState.State.Highlighted
                            : UIState.State.Default;
                    }
                })
                .AddTo(this);

            this.SubscribeButton(close, Close);
            this.SubscribeButton(reset, formation.Clear);
            this.SubscribeButton(auto, formation.AutoPlacement);
            this.SubscribeButton(confirm, OnConfirm);
        }

        public override UniTask Init(IEnumerable<UnitInfo> collection)
        {
            var exist = formation.Draft.Characters;

            return base.Init(collection
                .Where(unit => exist.FirstOrDefault(x => x.Id == unit.id) == null));
        }

        public void Open()
        {
            _completionSource = new();

            Entrance();
        }

        private void OnConfirm()
        {
            int fieldUnitCount = formation.Draft.Characters
                .OfType<UnitBehaviour>()
                .Count(x => x.Type == UnitBehaviour.BehaviourType.Unit);

            if (fieldUnitCount <= 0)
            {
                ServiceLocator.Get<CastleHero.View.Common.IPopupManager>().Open<PopupCommon>(ServiceLocator.Get<CastleHero.Common.Localize.LocalizeText>().Get(594));
                return;
            }

            _completionSource.TrySetResult();
        }

        private void Entrance()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOMoveY(-3.75f, 0.25f);
                cam.DOOrthoSize(12.5f, 0.25f);
            }

            animator.SetTrigger(UnFold);
            isOpened.Value = true;
        }

        public void Close()
        {
            _completionSource.TrySetCanceled();

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

            animator.SetTrigger(Fold);
            isOpened.Value = false;

            TaskHelper.OnAnimationEnd(animator, Fold)
                .ContinueWith(Dispose);
        }

        protected override UniTask SetItem(UICharacterSlot slot, UnitInfo data)
        {
            slot.OnClick += OnClickSlot;

            return base.SetItem(slot, data);
        }

        private void OnClickSlot(UISlot slot)
        {
            if (slot is not UICharacterSlot characterSlot)
                return;

            selected.Value = characterSlot;
        }
    }
}
