namespace CastleHero.Common.Sound
{
    public interface ISoundManager
    {
        void PlayBgm(string asset);
        void StopBgm();
        void PlaySfx(string asset);
    }
}
