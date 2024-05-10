using System.Collections.Generic;
using RGLabs.Common.UI;
using RGLabs.Stage.UI;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace RGLabs.Common.Behaviours
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

        [field: SerializeField] public UIStageSelect StageSelect;

        [SerializeField] private UIAtlasedSpriteCollection _spriteCollection;
        [SerializeField] private Button _button;
        [SerializeField] private Animator _animator;

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
            
            _animator.SetTrigger(ModeHash[value]);
        }

        private async void OnEnable()
        {
            await _spriteCollection.LoadAll();
            
            _button.enabled = true;
            
            _animator.SetTrigger(ActiveHash[true]);
        }

        private void OnDisable()
        {
            _button.enabled = false;

            _animator.SetTrigger(ActiveHash[false]);
        }

        #region Animation Events

        public void OnDisabled() => _spriteCollection.Dispose();

        #endregion
        
        public static implicit operator Button(StartButton it) => it._button;
    }
}