using CastleHero.Common.Behaviours;
using CastleHero.Common.Flow;

namespace CastleHero.Common
{
    public class Context
    {
        public BackButton Back { get; } = new();
        public StateManager<State> Transition { get; } = new(State.None);
        public StateManager<LobbyState> LobbyTransition { get; } = new(LobbyState.None);

        public void ProcessBack()
        {
            if (Back.ProcessBack(out var failedCause))
                return;

            if (failedCause == BackButton.FailedCause.NoListeners)
            {
                // TODO 게임 종료 팝업
            }
        }
    }
}
