using BackEnd;
using CastleHero.Network.Service;
using ErrorCode = CastleHero.Network.Error;

namespace CastleHero.Network.Impl.Backend
{
    public class BackendResult : Result
    {
        public BackendReturnObject raw;

        public static BackendResult Complete(BackendReturnObject raw) => new()
        {
            error = ErrorCode.None,
            raw = raw,
        };

        public new static BackendResult Error(ErrorCode error) => new() { error = error };

        public new static BackendResult Error(ErrorCode error, string errorMessage) => new()
        {
            error = error,
            errorMessage = errorMessage,
        };
    }

    public class BackendResult<T> : Result<T>
    {
        public BackendReturnObject raw;

        public static BackendResult<T> Complete(T data, BackendReturnObject raw) => new()
        {
            error = ErrorCode.None,
            data = data,
            raw = raw,
        };

        public new static BackendResult<T> Error(ErrorCode error) => new() { error = error };

        public new static BackendResult<T> Error(ErrorCode error, string errorMessage) => new()
        {
            error = error,
            errorMessage = errorMessage,
        };
    }
}
