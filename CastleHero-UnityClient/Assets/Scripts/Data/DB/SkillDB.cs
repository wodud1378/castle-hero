using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Rewards", menuName = "Scriptable Object/Rewards")]
    public class SkillDB : DB<SkillEntity>, IDataBase
    {
        protected override SkillEntity FallBackEntity() => default;

        public void Load(object[] data)
        {
            int length = data.Length;
            _entities = new SkillEntity[length];

            for (int i = 0; i < length; ++i)
            {
                _entities[i] = (SkillEntity)data[i];
            }
        }
    }
}