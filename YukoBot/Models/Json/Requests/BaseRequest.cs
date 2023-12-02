using System;

namespace YukoBot.Models.Json.Requests
{
    public class BaseRequest : BaseRequest<BaseRequest>
    {
    }

    public class BaseRequest<T> : Request<T>
    {
        public Guid Token { get; set; }
    }
}