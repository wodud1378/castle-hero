using System;

namespace RGLabs.Common.Pattern
{
    public class Observer<T> where T : struct
    {
        public event Action<T, T> OnChanged;
        
        public T It
        {
            get => _it;
            set
            {
                T legacy = _it;
                _it = value;
                
                OnChanged?.Invoke(legacy, value);
            }
        }
        
        private T _it;

        public Observer(T initialVal)
        {
            It = initialVal;
        }
        
        public static implicit operator T(Observer<T> observer) => observer.It;
    }
}