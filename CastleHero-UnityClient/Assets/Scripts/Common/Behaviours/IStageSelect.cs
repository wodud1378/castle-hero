using System;

namespace CastleHero.Common.Behaviours
{
    public interface IStageSelect : IDisposable
    {
        void Init();
        void SetMoveStageEnable(bool enabled);
    }
}
