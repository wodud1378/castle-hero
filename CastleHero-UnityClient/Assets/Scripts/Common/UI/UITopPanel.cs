using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UITopPanel : MonoBehaviour
    {
        private readonly int _lobbyHashId = Animator.StringToHash("Lobby");
        private readonly int _stageHashId = Animator.StringToHash("Stage");
        
        [SerializeField] private Animator _animator;

        private void Awake()
        {
            Context.Transition.StateObserver
                .Subscribe(x =>
                {
                    if (x == State.Shop)
                    {
                        gameObject.SetActive(false);
                        return;
                    }

                    gameObject.SetActive(true);

                    var hash = x switch
                    {
                        State.Lobby => _lobbyHashId,
                        State.Prepare => _stageHashId,
                        _ => 0
                    };
                    
                    _animator.SetTrigger(hash);
                })
                .AddTo(this);
        }
    }
}