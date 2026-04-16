using UnityEngine;

namespace CastleHero.Common.Sound
{
    [CreateAssetMenu(fileName = "SoundPath", menuName = "CastleHero/ScriptableObjects/Sound Path")]
    public class SoundPath : ScriptableObject
    {
        public string lobbyBgm;

        public string button;
        public string modifyFormation;
        public string back;
        public string bossWarning;
        public string gameClear;
        public string gameFailed;
        public string equipItem;
        public string setElemental;
        public string releaseItem;
        public string levelUp;
        public string openPopup;
        public string closePopup;
        public string useCurrency;
        public string purchaseDone;
        public string summonDirection;
        public string summonResult;
    }
}
