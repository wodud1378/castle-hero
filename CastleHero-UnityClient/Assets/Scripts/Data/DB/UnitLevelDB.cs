using RGLabs.Data.Model;

namespace RGLabs.Data.DB
{
    public class UnitLevelDB : DB<UnitLevelEntity>, IDataBase
    {
        protected override UnitLevelEntity FallBackEntity() => default;

        public void Load(object[] data)
        {
            int length = data.Length;
            _entities = new UnitLevelEntity[length];

            for (int i = 0; i < length; ++i)
            {
                _entities[i] = (UnitLevelEntity)data[i];
            }
        }
    }
}