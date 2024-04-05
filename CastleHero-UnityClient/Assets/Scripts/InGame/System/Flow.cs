using System;

namespace RGLabs.InGame.System
{
    public class Flow : IUpdate
    {
        public enum States
        {
            Loading,
            WaitForStart,
            Start,
            Update,
            End,
        }
        
        public Flow()
        {
            
        }
        
        public void Init()
        {
        }

        public void ProcessUpdate(float deltaTime)
        {
        }
    }
}