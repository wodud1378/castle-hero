using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using RGLabs.Common.UI;
using RGLabs.Data;
using RGLabs.Lobby.Behaviours;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace RGLabs.Stage.UI
{
    public class UIConfigCharacterList : UICharacterList
    {
        private static readonly int UnFold = Animator.StringToHash("UnFold");
        private static readonly int Fold = Animator.StringToHash("Fold");

        [SerializeField] private Animator _animator;
        [SerializeField] private FormationField _formation;
        [SerializeField] private Button _close;
        [SerializeField] private Button _reset;
        [SerializeField] private Button _auto;
        [SerializeField] private TMP_Text _placedUnit;
        [SerializeField] private float _dragThreshold = 0.3f;

        public readonly BoolReactiveProperty isOpened = new(false);
        public readonly ReactiveProperty<UICharacterSlot> selected = new(null);

        private Vector2 _startAt;
        private Vector2 _current;

        private float _holdTime;
        private bool _onHold;

        private CancellationTokenSource _ctSource;
        private int _originLayer;

        private void Awake()
        {
            Storage.inGameRepository.characters
                .ChangeAsObservable()
                .ThrottleFrame(1)
                .Subscribe(_ => Init(Storage.userRepository.characters))
                .AddTo(this);

            _formation.placed
                .CombineLatest(_formation.capacity, (current, max) => (current, max))
                .ThrottleFrame(1)
                .Subscribe(x => _placedUnit.text = $"{x.current}/{x.max}")
                .AddTo(this);

            selected
                .Subscribe(x =>
                {
                    foreach (var item in _items)
                        item.SetHighlight(item == x);
                })
                .AddTo(this);

            this.SubscribeButton(_close, Close);
            this.SubscribeButton(_reset, _formation.Clear);
            this.SubscribeButton(_auto, _formation.AutoPlacement);
        }

        public override UniTask Init(IEnumerable<UnitInfo> collection)
        {
            var exist = Storage.inGameRepository.characters;

            return base.Init(collection
                .Where(unit => exist.FirstOrDefault(x => x.Id == unit.id) == null));
        }

        public void Open() => Entrance();

        private void Entrance()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOMoveY(-3.75f, 0.25f);
                cam.DOOrthoSize(12.5f, 0.25f);
            }

            _animator.SetTrigger(UnFold);
            isOpened.Value = true;
        }

        private void Close() => Exit();

        private void Exit()
        {
            var cam = Camera.main;
            if (cam != null)
            {
                cam.transform.DOMoveY(0, 0.25f);
                cam.DOOrthoSize(15f, 0.25f);
            }

            _animator.SetTrigger(Fold);
            isOpened.Value = false;

            TaskHelper.OnAnimationEnd(_animator, Fold)
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