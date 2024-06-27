using System;

namespace RGLabs.Network.Model
{
    [Serializable]
    public class StageClear
    {
        public int stage;
        public int exp;
        public int gold;
        public bool isFirstClear;
        public Item[] acquired;
        public UnitInfo[] updated;
    }
}