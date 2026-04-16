using System;

namespace CastleHero.GamePlay.InGame.System.Wave.Creation
{
    public interface ICreationHelper
    {
        public void SetUpCreations(Action<UnitCreation> onCreation);
    }
}