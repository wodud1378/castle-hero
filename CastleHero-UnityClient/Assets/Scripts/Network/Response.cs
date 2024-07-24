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
        Maintenance,
        InvalidRequest
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
            // 로컬에서 null을 넣을 경우 모두 Success.
            if (obj == null)
                return ResultCode.Success;
            
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
        public delegate T Convert(BackendReturnObject raw);
        
        public readonly ResultCode result;
        public readonly BackendReturnObject raw;
        public readonly T data;
        
        public Response(BackendReturnObject raw, Convert convert = null) : base(raw)
        {
            if (result != ResultCode.Success)
                return;
            
            if (convert == null)
            {
                var json = raw.FlattenRows();
                var str = JsonMapper.ToJson(json);
                data = JsonMapper.ToObject<T>(str);   
            }
            else
            {
                data = convert.Invoke(raw);
            }
        }
    }
}