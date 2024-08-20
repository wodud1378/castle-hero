namespace RGLabs.Unit.Skill
{
    public abstract class ActiveSkill : SkillBase
    {
        public override void Init()
        {
            Runner.OnExecuteEvent -= OnExecute;
            Runner.OnExecuteEvent += OnExecute;

            Runner.OnReleaseEvent -= OnRelease;
            Runner.OnReleaseEvent += OnRelease;
        }

        protected abstract void OnExecute();
        
        private void OnRelease() =>  Cycle.StartWaiting();
    }
}