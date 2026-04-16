using System;
using UniRx;

namespace CastleHero.Common.Flow
{
    // 로비 내부 UI 수준 상태
    public enum LobbyState
    {
        None,
        Main,
        Shop,
        Castle,
        Prepare,
    }

    /// <summary>
    /// 제네릭 상태 관리자. Rx 기반으로 상태 변화 관찰 가능.
    /// 이전에는 Transition, LobbyTransition 클래스가 동일 구조로 중복되어 있었으나 하나로 통합됨.
    /// </summary>
    public class StateManager<T>
    {
        public T CurrentState
        {
            get => _currentState.Value;
            set => _currentState.Value = value;
        }

        public IObservable<T> StateObserver => _currentState.Share();

        private readonly ReactiveProperty<T> _currentState;

        public StateManager(T initial)
        {
            _currentState = new ReactiveProperty<T>(initial);
        }
    }
}
