// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Application.Common.Interfaces;

public interface IDateTime
{
    DateTime Now { get; }
    DateTime Today { get; }
    DateTime Yesterday { get; }
    DateTime Tomorrow { get; }
    DateTime TwoDaysBack { get; }
}
