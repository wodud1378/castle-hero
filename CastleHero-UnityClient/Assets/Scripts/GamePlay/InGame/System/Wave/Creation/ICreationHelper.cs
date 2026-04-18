using System;

namespace CastleHero.GamePlay.InGame.System.Wave.Creation
{
    public interface ICreationHelper : IDisposable
    {
        public void SetUpCreations(Action<UnitCreation> onCreation);
    }
}