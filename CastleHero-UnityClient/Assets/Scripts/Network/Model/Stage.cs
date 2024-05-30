using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public struct StageClear
    {
        public int stage;
        public int exp;
        public int gold;
        public bool isFirstClear;
        public Item[] items;
        public CharacterGrowth[] growths;
    }

    [Serializable]
    public struct CharacterGrowth
    {
        public int prevLv;
        public int currentLv;

        public int lastExp;
        public int currentExp;
    }
}