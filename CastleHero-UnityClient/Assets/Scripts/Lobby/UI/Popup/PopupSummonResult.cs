using Cysharp.Threading.Tasks;
using RGLabs.Common.UI.Popup;
using RGLabs.Lobby.UI.Adapter;
using RGLabs.Network.Shared;
using RGLabs.Utility;
using UnityEngine;

namespace RGLabs.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/Summon/PopupSummonResult.prefab")]
    public class PopupSummonResult : PopupBase
    {
        [SerializeField] private UISummonedList _summonedList;

        public override UniTask Open(params object[] parameters)
        {
            var data = (Summon)parameters[0];

            return _summonedList.Init(data.list);
        }
    }
}