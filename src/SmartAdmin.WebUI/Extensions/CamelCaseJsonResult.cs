// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace System
{
    public class CamelCaseJsonResult : JsonResult
    {
        public CamelCaseJsonResult(object value) : base(value)
        {
            this.SerializerSettings = SystemExtensions.CamelCaseJsonPolicy;
        }

        public CamelCaseJsonResult(object? value, object? serializerSettings)
            : base(value, serializerSettings) 
        {
            
        }
    }
}
