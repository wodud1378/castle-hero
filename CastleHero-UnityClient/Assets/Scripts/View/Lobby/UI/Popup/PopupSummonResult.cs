using Cysharp.Threading.Tasks;
using CastleHero.View.Common.UI.Popup;
using CastleHero.View.Lobby.UI.Adapter;
using CastleHero.Network.Shared;
using CastleHero.Utility;
using UnityEngine;
using UnityEngine.Serialization;

namespace CastleHero.View.Lobby.UI.Popup
{
    [PrefabPath("Lobby/UI/Prefabs/Popups/PopupSummonResult.prefab")]
    public class PopupSummonResult : PopupBase
    {
        [FormerlySerializedAs("_summonedList")]
        [SerializeField] private UISummonedList summonedList;

        public override UniTask Open(params object[] parameters)
        {
            var data = (Summon)parameters[0];

            return summonedList.Init(data.list);
        }
    }
}
