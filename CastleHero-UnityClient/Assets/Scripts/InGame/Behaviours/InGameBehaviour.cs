using UnityEngine;

namespace RGLabs.InGame
{
    public class InGameBehaviour : MonoBehaviour
    {
        private PrepareHelper _prepare;

        private void Awake()
        {
            _prepare = new();
        }

        public async void StartPrepareSteps()
        {
            _prepare.PrepareAsync()
        }
    }
}