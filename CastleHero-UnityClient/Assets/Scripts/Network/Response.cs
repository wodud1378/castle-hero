using BackEnd;
using Newtonsoft.Json;
using RGLabs.Network.Parse;

namespace RGLabs.Network
{
    public enum ResultCode
    {
        Success,
        NetworkError,
        AuthenticationError,
        ServerError,
        UnknownError,
        InitializationFailed,
        Maintenance
    }

    public class Response
    {
        public readonly ResultCode result;
        public readonly BackendReturnObject row;

        public Response(BackendReturnObject row)
        {
            this.row = row;

            result = GetResult(row);
        }
        
        private ResultCode GetResult(BackendReturnObject obj)
        {
            if (obj.IsSuccess())
                return ResultCode.Success;

            switch (obj.GetErrorCode())
            {
                case "NetworkError": return ResultCode.NetworkError;
                case "UnauthorizedException": return ResultCode.AuthenticationError;
                case "ServerException": return ResultCode.ServerError;
                case "Maintenance": return ResultCode.Maintenance;
                default: return ResultCode.UnknownError;
            }
        }
    }
    
    public class Response<T> : Response
    {
        public readonly ResultCode result;
        public readonly BackendReturnObject row;
        public readonly T data;
        
        private readonly IParser<T> _parser;
        
        public Response(BackendReturnObject row, IParser<T> parser = null) : base(row)
        {
            if (result != ResultCode.Success)
                return;
            
            _parser = parser;
            var json = row.GetReturnValue();
            data = JsonConvert.DeserializeObject<T>(json);
        }
    }
}