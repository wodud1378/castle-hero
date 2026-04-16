using System;

namespace CastleHero.Network.Service.Boot
{
    [Serializable]
    public struct ChartInfo
    {
        public string chartName;
        public string chartExplain;
        public int selectedChartFileId;
    }
}
