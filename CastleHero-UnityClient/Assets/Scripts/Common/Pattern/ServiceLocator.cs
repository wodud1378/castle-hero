using System;
using System.Collections.Generic;

namespace CastleHero.Common.Pattern
{
    /// <summary>
    /// 경량 서비스 로케이터. 타입 기반으로 런타임 서비스를 등록/조회한다.
    /// Storage/Context 의 산재된 static 필드들을 점진적으로 대체하는 역할.
    /// 등록은 부팅 초기화 경로(BackendBootService, Context.LoadAsync) 한 곳에서만 수행하는 것을 권장.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new();

        /// <summary>
        /// 특정 타입 등록 시 호출되는 이벤트. 확장 메서드 같이 "매 호출 Get 을 피해야 하는" 유틸이
        /// 최초 등록 시점에 캐시를 업데이트하는 용도.
        /// </summary>
        public static event Action<Type, object> OnRegistered;

        public static void Register<T>(T service) where T : class
        {
            if (service == null)
                throw new ArgumentNullException(nameof(service));
            _services[typeof(T)] = service;
            OnRegistered?.Invoke(typeof(T), service);
        }

        /// <summary>
        /// 등록된 서비스를 반환. 등록되지 않았으면 InvalidOperationException.
        /// 초기화 순서를 강제하기 위해 실패를 명시적으로 드러낸다.
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (_services.TryGetValue(typeof(T), out var service))
                return (T)service;

            throw new InvalidOperationException(
                $"ServiceLocator: '{typeof(T).Name}' 서비스가 등록되지 않았습니다. " +
                $"부팅 초기화 경로에서 Register<{typeof(T).Name}>() 가 호출되었는지 확인하세요.");
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            if (_services.TryGetValue(typeof(T), out var obj))
            {
                service = (T)obj;
                return true;
            }

            service = null;
            return false;
        }

        public static void Unregister<T>() where T : class
        {
            _services.Remove(typeof(T));
        }

        /// <summary>
        /// 테스트 또는 씬 리로드 시 사용. 기본적으로 호출하지 않는다.
        /// </summary>
        public static void Clear() => _services.Clear();
    }
}
