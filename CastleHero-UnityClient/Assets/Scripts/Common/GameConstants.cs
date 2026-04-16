using UnityEngine;

namespace CastleHero.Common
{
    /// <summary>
    /// 런타임 튜닝 가능한 게임 전역 설정. Inspector 에서 수정 가능.
    /// Bootstrap (Context.LoadAsync 또는 BootService) 이 GameConstants 에셋을 로드해 ServiceLocator 에 등록,
    /// 소비자는 생성자/Init 으로 주입받아 사용한다.
    /// 컴파일 타임에 필요한 값 (switch-case, 배열 초기자) 은 <see cref="CastleHero.Constants"/> 에 그대로 유지.
    /// </summary>
    [CreateAssetMenu(fileName = "GameConstants", menuName = "ScriptableObjects/GameConstants")]
    public class GameConstants : ScriptableObject
    {
        [Header("Durations & Thresholds")]
        public float hitEffectDuration = 0.15f;
        public float dragDistanceThreshold = 0.3f;
        public float defaultObjectAngle = 0f;

        [Header("Format")]
        public string coloredStringTag = "<color={0}>{1}</color>";

        [Header("Barricade Resources")]
        public string barricadeIcon = "Common/Portrait/Character_10000.png";
        public string barricadePrefab = "Character_10000/Character_10000.prefab";

        [Header("Currency Icons")]
        public string goldIcon = "Common/Icon/Icon_Gold.png";
        public string diaIcon = "Common/Icon/Icon_Diamond.png";
        public string expIcon = "Common/Icon/Icon_Exp.png";

        [Header("Effect Keys")]
        public string manaDropEffect = "Effect_ManaStone";
        public string deadEffect = "UnitEffect/Dead/Dead.prefab";
        public string recoverEffect = "UnitEffect/Recover/Recover.prefab";
        public string spawnEffect = "UnitEffect/Spawn/Spawn.prefab";
    }
}
