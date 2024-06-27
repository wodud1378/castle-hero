namespace RGLabs.Network.Parse
{
    public interface IParser<out T>
    {
        T Parse(string json);
    }
}