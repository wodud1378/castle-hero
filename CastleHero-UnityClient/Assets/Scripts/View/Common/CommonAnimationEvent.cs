using UnityEngine;
using UnityEngine.Events;

namespace CastleHero.Common.Behaviours
{
    public class CommonAnimationEvent : MonoBehaviour
    {
         public UnityEvent onStart;
         public UnityEvent onEnd;

         public void OnStart() => onStart?.Invoke();
         public void OnEnd() => onEnd?.Invoke();
    }
}