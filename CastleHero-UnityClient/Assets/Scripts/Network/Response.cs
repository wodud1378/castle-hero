using BackEnd;
using LitJson;
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
        public readonly BackendReturnObject raw;

        public Response(BackendReturnObject raw)
        {
            this.raw = raw;

            result = GetResult(raw);
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
        public readonly BackendReturnObject raw;
        public readonly T data;
        
        private readonly IParser<T> _parser;
        
        public Response(BackendReturnObject raw, IParser<T> parser = null) : base(raw)
        {
            if (result != ResultCode.Success)
                return;
            
            _parser = parser;
            var json = raw.FlattenRows();
            var str = JsonMapper.ToJson(json);
            data = JsonConvert.DeserializeObject<T>(str);
        }
    }
}