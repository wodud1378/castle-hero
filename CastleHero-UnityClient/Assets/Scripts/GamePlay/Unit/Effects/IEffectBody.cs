namespace CastleHero.GamePlay.Unit.Effects
{
    public interface IEffectBody
    {
        void Attach(IEffect effect);
        void Clear();
    }
}
