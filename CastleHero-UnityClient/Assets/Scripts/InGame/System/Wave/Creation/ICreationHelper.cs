using System;

namespace RGLabs.InGame.System.Wave.Creation
{
    public interface ICreationHelper
    {
        public void SetUpCreations(Action<UnitCreation> onCreation);
    }
}