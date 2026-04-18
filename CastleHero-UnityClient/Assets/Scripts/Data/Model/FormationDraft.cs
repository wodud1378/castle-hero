using System;
using CastleHero.Common.Behaviours;
using UniRx;

namespace CastleHero.Data.Model
{
    /// <summary>
    /// 로비 Prepare 단계에서 유저가 배치한 유닛/성의 씬 인스턴스 컬렉션.
    /// FormationField 가 소유하며, StartGame 메시지를 통해 InGameSession 에 전달된다.
    /// 인게임 전용 런타임 상태(InGameSession)와 분리된 "준비 단계 배치 결과" 역할.
    /// </summary>
    public class FormationDraft : IDisposable
    {
        public ReactiveCollection<IUnitActor> Characters { get; } = new();
        public ReactiveProperty<IUnitActor> Castle { get; } = new(null);

        public void Dispose()
        {
            Characters?.Dispose();
            Characters?.Clear();
            Castle?.Dispose();
        }
    }
}
