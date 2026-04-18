using CastleHero.Common.Behaviours;
using CastleHero.View.Common;
using CastleHero.View.Bootstrapper;
using CastleHero.Common.Flow;
using UniRx;
using UnityEngine;
using UnityEngine.Serialization;
using CastleHero.Common.Pattern;
using CastleHero.Data.Model;

namespace CastleHero.View.Common.UI
{
    public class UITopPanel : MonoBehaviour
    {
        private readonly int _entranceHashId = Animator.StringToHash("Entrance");
        private readonly int _exitHashId = Animator.StringToHash("Exit");

        private readonly int _lobbyHashId = Animator.StringToHash("Lobby");
        private readonly int _stageHashId = Animator.StringToHash("Stage");

        [FormerlySerializedAs("_animator")]
        [SerializeField] private Animator animator;

        private StateManager<State> _stateManager;
        private StateManager<LobbyState> _lobbyStateManager;

        private void Awake()
        {
            var sl = ServiceLocator.Instance;
            _stateManager = sl.Get<StateManager<State>>();
            _lobbyStateManager = sl.Get<StateManager<LobbyState>>();

            _stateManager.StateObserver
                .Where(x => x == State.InGame)
                .Subscribe(_ => TransitionToExit())
                .AddTo(this);

            _lobbyStateManager.StateObserver
                .Where(x => x != LobbyState.None)
                .Subscribe(TransitionTo)
                .AddTo(this);
        }

        private void TransitionToExit()
        {
            animator.SetTrigger(_exitHashId);
        }

        private void TransitionTo(LobbyState state)
        {
            bool isActive = state is not LobbyState.Shop;
            animator.SetTrigger(isActive ? _entranceHashId : _exitHashId);

            if (!isActive)
                return;

            var hash = state switch
            {
                LobbyState.Main => _lobbyHashId,
                LobbyState.Prepare => _stageHashId,
                _ => 0
            };

            animator.SetTrigger(hash);
        }
    }
}
