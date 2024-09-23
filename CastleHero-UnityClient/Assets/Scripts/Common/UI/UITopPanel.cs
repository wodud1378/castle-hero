using RGLabs.Common.Behaviours;
using RGLabs.Common.Flow;
using UniRx;
using UnityEngine;

namespace RGLabs.Common.UI
{
    public class UITopPanel : MonoBehaviour
    {
        private readonly int _entranceHashId = Animator.StringToHash("Entrance");   
        private readonly int _exitHashId = Animator.StringToHash("Exit");   
        
        private readonly int _lobbyHashId = Animator.StringToHash("Lobby");
        private readonly int _stageHashId = Animator.StringToHash("Stage");
        
        [SerializeField] private Animator _animator;

        private void Awake()
        {
            Context.Transition.StateObserver
                .Subscribe(x =>
                {
                    bool isActive = x is not State.Shop and not State.InGame;
                    _animator.SetTrigger(isActive ? _entranceHashId : _exitHashId);
                    
                    if (!isActive)
                        return;

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