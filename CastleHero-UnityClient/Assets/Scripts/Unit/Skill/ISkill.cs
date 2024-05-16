namespace RGLabs.Unit.Skill
{
    public interface ISkill
    {
        public bool IsReady();
        public bool TryExecute();
    }
}