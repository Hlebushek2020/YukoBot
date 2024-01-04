﻿using System;
using YukoClientBase.Enums;
using YukoClientBase.Extensions;

namespace YukoClientBase.Exceptions
{
    public class ClientCodeException : Exception
    {
        public ClientErrorCodes ClientErrorCode { get; }

        public ClientCodeException(ClientErrorCodes clientErrorCode) : base(clientErrorCode.GetText())
        {
            ClientErrorCode = clientErrorCode;
        }

        public ClientCodeException(ClientErrorCodes clientErrorCode, params object[] args)
            : base(clientErrorCode.GetText(args))
        {
            ClientErrorCode = clientErrorCode;
        }
    }
}