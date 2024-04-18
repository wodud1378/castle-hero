using UnityEngine;

namespace RGLabs.InGame.Common
{
    public static class Constants
    {
        public static readonly int spawnBufferSize = 50;
        public static readonly int spawnLimitPerFrame = 2;
        
        public static int IdleAnim { get; private set; }
        public static int MoveAnim { get; private set; }
        public static int AtkAnim { get; private set; }
        public static int SkillAnim { get; private set; }
        public static int DeadAnim { get; private set; }

        
        [RuntimeInitializeOnLoadMethod]
        private static void InitializeAnimationKeys()
        {
            IdleAnim = Animator.StringToHash("Idle");
            MoveAnim = Animator.StringToHash("Move");
            AtkAnim = Animator.StringToHash("Attack");
            SkillAnim = Animator.StringToHash("Skill");
            DeadAnim = Animator.StringToHash("Dead");
        }
    }
}