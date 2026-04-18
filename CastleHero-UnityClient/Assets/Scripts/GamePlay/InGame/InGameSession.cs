using System;
using CastleHero.Common.Behaviours;
using CastleHero.Data.Model;
using CastleHero.Data.Repositories;
using UniRx;

namespace CastleHero.GamePlay.InGame
{
    /// <summary>
    /// 실제 전투(InGame)가 진행되는 동안만 유효한 런타임 세션 상태.
    /// InGameScene.Run 에서 생성되고 OnExit/Dispose 에서 파괴된다.
    /// Prepare 단계에서 배치한 유닛/성은 FormationDraft 에 담겨 전달되며,
    /// 세션은 해당 컬렉션 참조만 위임 노출한다 (소유권은 FormationField 의 FormationDraft 에 있음).
    /// </summary>
    public interface IInGameSession : IDisposable
    {
        IGameEntity GameEntity { get; set; }
        ReactiveProperty<int> Mana { get; }
        ReactiveProperty<IUnitActor> Castle { get; }
        ReactiveCollection<IUnitActor> Characters { get; }
        ReactiveCollection<WaitRecover> Recovers { get; }
        ReactiveProperty<float> LeftTime { get; }
    }

    public class InGameSession : IInGameSession
    {
        /// <summary>
        /// 전투 진행 중에만 non-null. InGameScene.Run 에서 설정, Dispose 에서 해제.
        /// </summary>
        public static IInGameSession Current { get; private set; }

        public IGameEntity GameEntity { get; set; }
        public ReactiveProperty<int> Mana { get; } = new(0);
        public ReactiveCollection<WaitRecover> Recovers { get; } = new();
        public ReactiveProperty<float> LeftTime { get; } = new();

        public ReactiveProperty<IUnitActor> Castle => _draft.Castle;
        public ReactiveCollection<IUnitActor> Characters => _draft.Characters;

        private readonly FormationDraft _draft;

        public InGameSession(FormationDraft draft)
        {
            _draft = draft ?? throw new ArgumentNullException(nameof(draft));
            Current = this;
        }

        public void Dispose()
        {
            Recovers?.Dispose();
            Recovers?.Clear();

            Mana?.Dispose();
            LeftTime?.Dispose();

            if (ReferenceEquals(Current, this))
                Current = null;

            // _draft 는 FormationField 가 소유하므로 여기서 Dispose 하지 않는다.
        }
    }
}
