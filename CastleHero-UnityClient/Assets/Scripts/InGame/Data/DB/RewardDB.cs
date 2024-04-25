using RGLabs.InGame.Data.Model;
using UnityEngine;

namespace RGLabs.InGame.Data.DB
{
    [CreateAssetMenu(fileName = "Rewards", menuName = "Scriptable Object/Rewards")]
    public class RewardDB : DB<RewardEntity>
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