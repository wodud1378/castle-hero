using System;

namespace RGLabs.Unit.Finding
{
    public class DetectionFactory
    {
        public IDetection GetDetection(IDetection.Option option)
        {
            switch (option)
            {
                case IDetection.Option.Circle:
                    return new CircleDetection();
                case IDetection.Option.Arc:
                    return new ArcDetection();
                case IDetection.Option.Box:
                    return new BoxDetection();
                default:
                    throw new ArgumentOutOfRangeException(nameof(option), option, null);
            }
        }
    }
}