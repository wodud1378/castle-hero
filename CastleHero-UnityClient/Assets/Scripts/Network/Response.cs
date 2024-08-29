using System;
using BackEnd;
using LitJson;
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
    }
    
    public enum Error
    {
        None,
        FromNetwork,
        Unauthorized,
        Maintenance,
        Unknown,
        FromServer,
        InvalidRequest,
        
        // Common.
        ChartLoadFailed,
        DBReadFailed,
        DBWriteFailed,
        DataNotFound,
        InvalidData,
        NotEnoughCurrency,
        NotEnoughItem,
        UnitNotFound,

        // InGame.
        NotEnoughAp,
        NotOpened,
        InvalidDayOfWeek,

        // Character.
        AlreadyEquipped,
        NotEquipped,

        // Unit (Castle, Character).
        AlreadyMaxLv,

        // Shop.
        SoldOut,
        SoldOutByType,
        InvalidPaymentType,
        InvalidDate,
        NotOpenedProduct,
    }

    public class Response
    {
        public bool IsSuccess => error == Error.None;

        public readonly int statusCode;
        public readonly Error error;
        public readonly BackendReturnObject raw;

        public Response(BackendReturnObject raw, Error error = Error.None)
        {
            statusCode = int.Parse(raw.GetStatusCode());
            this.error = error;

            this.raw = raw;

            Debug.Log(raw.HasReturnValue()
                ? raw.GetReturnValuetoJSON().ToJson()
                : raw.GetMessage());
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
            if (!IsSuccess)
                return;

            if (convert == null)
            {
                var str = JsonMapper.ToJson(base.raw);
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