using RGLabs.Network.Service;
using UnityEngine;

namespace RGLabs.Title
{
    public class TitleBehaviour : MonoBehaviour
    {
        private async void Awake()
        {
            INetworkService network = new LocalNetworkService();
            
            var unitInfo = await network.Login();
            
            
        }

        private async void Start()
        {
            
        }
    }
}