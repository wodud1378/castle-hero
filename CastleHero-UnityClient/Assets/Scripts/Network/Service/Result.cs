using ErrorCode = CastleHero.Network.Error;

namespace CastleHero.Network.Service
{
    public class Result
    {
        public bool IsSuccess => error == ErrorCode.None;

        public ErrorCode error;
        public string errorMessage;
        public int statusCode;

        public static Result Complete() => new() { error = ErrorCode.None };

        public static Result Error(ErrorCode error) => Error(error, string.Empty);

        public static Result Error(ErrorCode error, string errorMessage) =>
            new() { error = error, errorMessage = errorMessage };
    }

    public class Result<T> : Result
    {
        public T data;

        public static Result<T> Complete(T data) => new() { data = data, error = ErrorCode.None };

        public new static Result<T> Error(ErrorCode error) => Error(error, string.Empty);

        public new static Result<T> Error(ErrorCode error, string errorMessage) =>
            new() { error = error, errorMessage = errorMessage };

        public static Result<T> Error(ErrorCode error, int statusCode) =>
            new() { error = error, statusCode = statusCode };
    }
}
