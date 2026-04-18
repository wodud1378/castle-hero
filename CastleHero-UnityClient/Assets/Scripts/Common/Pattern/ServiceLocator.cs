using System;
using System.Collections.Generic;

namespace CastleHero.Common.Pattern
{
    /// <summary>
    /// 경량 서비스 로케이터. 타입 기반으로 런타임 서비스를 등록/조회한다.
    /// 등록은 부트 단일 지점(Context.LoadAsync, BackendBootService, *Installer)에서만 수행한다.
    /// 조회는 plain C# 생성자 / MonoBehaviour Awake|Init / static ctor+OnRegistered 에서만 허용.
    /// </summary>
    public sealed class ServiceLocator : IServiceLocator
    {
        public static IServiceLocator Instance { get; } = new ServiceLocator();

        private readonly Dictionary<Type, object> _services = new();

        public event Action<Type, object> OnRegistered;

        private ServiceLocator() { }

        public void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
            OnRegistered?.Invoke(typeof(T), service);
        }

        public T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"ServiceLocator: '{typeof(T).Name}' 서비스가 등록되지 않았습니다. " +
                $"부트 초기화 경로에서 Register<{typeof(T).Name}>() 가 호출되었는지 확인하세요.");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        public void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        public void Clear() => _services.Clear();
    }
}
