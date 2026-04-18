using UnityEngine;
using CastleHero.GamePlay.Unit.Behaviours;

namespace CastleHero.View.Lobby.Behaviours
{
    /// <summary>
    /// FormationField 의 Scene 의존 기능을 추상화한 인터페이스.
    /// FormationComposer 가 배치 유효성 / 장애물 등록을 위해 호출한다.
    /// </summary>
    public interface IFormationFieldArea
    {
        float AutoPlacementRadius { get; }
        bool IsValid(Collider2D collider, int layer);
        void AddObstacle(UnitActor unit, bool regenerateMap = true);
        void RemoveObstacle(UnitActor unit);
        void GenerateMap();
    }
}
