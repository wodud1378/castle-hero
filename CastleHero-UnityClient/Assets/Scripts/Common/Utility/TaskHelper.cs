using System;
using Cysharp.Threading.Tasks;
using UniRx;
using UnityEngine;

namespace CastleHero.Utility
{
    public static class TaskHelper
    {
        public static UniTask OnAnimationEnd(Animator animator, int hash)
        {
            animator.SetTrigger(hash);

            return Observable
                .EveryUpdate()
                .Where(_ =>
                {
                    if (animator == null)
                        return true;
                    
                    var state = animator.GetCurrentAnimatorStateInfo(0);
                    return state.shortNameHash == hash && state.normalizedTime >= 1f;
                })
                .First()
                .ToUniTask();
        }
    }
}