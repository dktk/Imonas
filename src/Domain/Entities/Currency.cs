// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Domain.Entities
{
    public class Currency : AuditableEntity
    {
        public Currency()
        {
            
        }

        public Currency(string name, string code, string htmlCode)
        {
            Name = name;
            Code = code;
            HtmlCode = htmlCode;
        }

        public string Name { get; set; }
        public string Code     {get;set;}
        public string HtmlCode {get;set;}
    }
}
