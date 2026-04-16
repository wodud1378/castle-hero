using System.Collections.Generic;

namespace CastleHero.Common.Flow
{
    public interface IBackButtonListener
    {
        public bool OnProcessBack();
    }

    public class BackButton
    {
        public enum FailedCause
        {
            None,
            Disabled,
            NoListeners,
            ProcessFailed
        }
        
        public bool HasListeners => _listeners.Count > 0;
        public bool enabled = true;

        private readonly List<IBackButtonListener> _listeners = new();

        public void Add(IBackButtonListener listener) => _listeners.Insert(0, listener);
        
        public void Remove(IBackButtonListener listener) => _listeners.Remove(listener);

        public void Clear() => _listeners.Clear();

        public bool ProcessBack(out FailedCause cause)
        {
            if (!enabled)
            {
                cause = FailedCause.Disabled;
                return false;
            }
            
            _listeners.RemoveAll((x) => x == null);
            if (!HasListeners)
            {
                cause = FailedCause.NoListeners;
                return false;
            }

            var back = _listeners[0];
            if (!back.OnProcessBack())
            {
                cause = FailedCause.ProcessFailed;
                return false;
            }

            _listeners.Remove(back);
            
            cause = FailedCause.None;
            return true;
        }
    }
}