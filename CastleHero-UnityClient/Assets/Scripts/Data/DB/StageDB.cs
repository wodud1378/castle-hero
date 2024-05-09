using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Stages", menuName = "Scriptable Object/Stages")]
    public class StageDB : DB<StageEntity>, IDataBase
    {
        protected override StageEntity FallBackEntity() => new()
        {
        };
    }
}