using System;
using UniRx;

namespace RGLabs.Common.Flow
{
    public enum State
    {
        None,
        Lobby,
        Shop,
        Stage,
        InGame,
    }
    
    public class Transition
    {
        public State LastState { get; private set; } = State.None;

        public State CurrentState
        {
            get => _currentState.Value;
            set
            {
                LastState = _currentState.Value;
                _currentState.Value = value;
            }
        }

        public IObservable<State> StateObserver => _currentState.Share();
        
        private readonly ReactiveProperty<State> _currentState = new(State.None);
    }
}