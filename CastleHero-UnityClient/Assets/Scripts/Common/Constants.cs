namespace CastleHero
{
    /// <summary>
    /// 컴파일 타임 상수 (const). switch-case, 패턴 매칭, 배열 크기 초기자 등에 필요한 값만 유지.
    /// 런타임에 튜닝 가능한 값은 <see cref="CastleHero.Common.GameConstants"/> ScriptableObject 를 사용할 것.
    /// </summary>
    public static class Constants
    {
        // Currency IDs — switch/pattern 에서 사용하므로 const 필수.
        public const int PaidDiaId = 1;
        public const int FreeDiaId = 2;
        public const int GoldId = 3;

        // Unit 버퍼 크기 — 배열 초기자에 사용하므로 const 필수.
        public const int BufferSize = 50;

        // Barricade Unit ID — 다수 파일에서 비교에 사용, 디자인 결정 상수.
        public const int BarricadeId = 10000;
    }
}
