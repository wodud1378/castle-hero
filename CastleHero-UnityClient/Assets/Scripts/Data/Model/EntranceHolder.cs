using CastleHero.Common.Flow;

using CastleHero.Common.Pattern;
using CastleHero.Data.Model;
namespace CastleHero.Data.Model
{
    /// <summary>
    /// 이전 ServiceLocator.Get<EntranceHolder>().Current static 필드를 대체.
    /// 앱 시작 ~ 종료까지 유지되며, ServiceLocator 에 등록되어 상태 머신이 가변 상태를 공유한다.
    /// </summary>
    public class EntranceHolder
    {
        public Entrance Current = new() { state = State.Lobby };
    }
}
