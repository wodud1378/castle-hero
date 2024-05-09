using RGLabs.Data.Model;
using UnityEngine;

namespace RGLabs.Data.DB
{
    [CreateAssetMenu(fileName = "Rewards", menuName = "Scriptable Object/Rewards")]
    public class RewardDB : DB<RewardEntity>, IDataBase
    {
        protected override RewardEntity FallBackEntity() =>
            new()
            {
                Id = -1,
                itemId = -1,
                minQuantity = 0,
                maxQuantity = 0,
            };
    }
}