// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Application.Settings;

namespace Application.Common.Interfaces;

public interface IMailService
{
    Task SendAsync(MailRequest request);
}
