using System;
using BackEnd;
using LitJson;
using Newtonsoft.Json;
using RGLabs.Network.Parse;
using RGLabs.Utility;
using UnityEngine;

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
        public bool IsSuccess => result == ResultCode.Success;

        public int statusCode;
        public ResultCode result;
        public JsonData rawData;

        public Response(BackendReturnObject raw)
        {
            result = GetResult(raw);
            statusCode = int.Parse(raw.GetStatusCode());
            rawData = raw.HasReturnValue()
                ? raw.FlattenRows()
                : null;

            Debug.Log(raw.HasReturnValue()
                ? raw.GetReturnValuetoJSON().ToJson()
                : raw.GetMessage());
        }

        private ResultCode GetResult(BackendReturnObject obj)
        {
            return obj.IsSuccess()
                ? ResultCode.Success
                : obj.GetErrorCode() switch
                {
                    "NetworkError" => ResultCode.NetworkError,
                    "UnauthorizedException" => ResultCode.AuthenticationError,
                    "ServerException" => ResultCode.ServerError,
                    "Maintenance" => ResultCode.Maintenance,
                    _ => ResultCode.UnknownError
                };
        }
    }

    public class ResponseDto<T>
    {
        public T data;
        public string error;
        public string errorDetail;
    }

    public class Response<T> : Response
    {
        public delegate T Convert(BackendReturnObject jsonData);

        public readonly T data;
        
        public Response(BackendReturnObject raw, Convert convert = null) : base(raw)
        {
            if (result != ResultCode.Success)
                return;

            if (convert == null)
            {
                var str = JsonMapper.ToJson(rawData);
                data = JsonMapper.ToObject<T>(str);
            }
            else
            {
                data = IsSuccess
                    ? convert.Invoke(raw)
                    : default;
            }

            Debug.Log(data.ToJson());
        }
    }
}