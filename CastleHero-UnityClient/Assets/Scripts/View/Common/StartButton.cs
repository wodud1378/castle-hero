using System;
using System.Collections.Generic;
using CastleHero.Common.Behaviours;
using CastleHero.View.Common.UI;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace CastleHero.View.Common
{
    public class StartButton : MonoBehaviour
    {
        public enum Mode
        {
            Lobby,
            Stage
        }

        private static readonly Dictionary<Mode, int> ModeHash = new()
        {
            { Mode.Lobby, Animator.StringToHash(Mode.Lobby.ToString()) },
            { Mode.Stage, Animator.StringToHash(Mode.Stage.ToString()) },
        };

        private static readonly Dictionary<bool, int> ActiveHash = new()
        {
            { true, Animator.StringToHash("Appear") },
            { false, Animator.StringToHash("Disappear") }
        };

        [FormerlySerializedAs("StageSelectBehaviour")]
        [field: SerializeField] public MonoBehaviour StageSelectComponent;
        public IStageSelect StageSelect => (IStageSelect)StageSelectComponent;

        [FormerlySerializedAs("_button")]
        [SerializeField] private Button button;
        [FormerlySerializedAs("_animator")]
        [SerializeField] private Animator animator;

        public readonly ReactiveProperty<Mode> mode = new(Mode.Lobby);

        private void Start()
        {
            mode
                .DistinctUntilChanged()
                .Subscribe(OnModeChanged)
                .AddTo(this);
        }

        private void OnModeChanged(Mode value)
        {
            StageSelect.SetMoveStageEnable(value == Mode.Lobby);

            animator.SetTrigger(ModeHash[value]);
        }

        private void OnEnable()
        {
            //await _spriteCollection.LoadAll();

            button.enabled = true;

            animator.SetTrigger(ActiveHash[true]);
        }

        private void OnDisable()
        {
            button.enabled = false;

            animator.SetTrigger(ActiveHash[false]);
        }

        private void OnDestroy()
        {
            StageSelect.Dispose();
        }

        #region Animation Events

        public void OnDisabled()
        {
            //_spriteCollection.Dispose();
        }

        #endregion

        public static implicit operator Button(StartButton it) => it.button;
    }
}
