﻿using System;

namespace SimpleIdServer.Module
{
    public class ModuleException : Exception
    {
        public ModuleException(string code, string message) : base(message)
        {
            Code = code;
        }

        public string Code { get; private set; }
    }
}
